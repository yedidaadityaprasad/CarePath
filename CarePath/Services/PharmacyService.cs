using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class PharmacyService : IPharmacyService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public PharmacyService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<IReadOnlyList<WardStock>> ListStockAsync(int? wardId, bool lowStockOnly)
    {
        var query = _db.WardStocks.AsNoTracking().Include(s => s.Ward).Include(s => s.Drug).AsQueryable();
        if (wardId.HasValue)
        {
            query = query.Where(s => s.WardId == wardId.Value);
        }
        if (lowStockOnly)
        {
            query = query.Where(s => s.QuantityOnHand <= s.ReorderLevel);
        }
        return await query.OrderBy(s => s.Ward.Code).ThenBy(s => s.Drug.Name).ToListAsync();
    }

    public async Task<WardStockDetailsViewModel> GetStockDetailsAsync(int id)
    {
        var stock = await _db.WardStocks.AsNoTracking().Include(s => s.Ward).Include(s => s.Drug).GetRequiredAsync(id, "Ward stock");
        var movements = await _db.StockMovements.AsNoTracking()
            .Where(m => m.WardStockId == id)
            .OrderByDescending(m => m.OccurredAt).ThenByDescending(m => m.Id)
            .Take(100)
            .ToListAsync();
        return new WardStockDetailsViewModel { Stock = stock, Movements = movements };
    }

    public async Task<int> CreateStockAsync(WardStockFormViewModel model)
    {
        var ward = await _db.Wards.GetRequiredAsync(model.WardId ?? 0, "Ward", nameof(model.WardId));
        var drug = await _db.Drugs.GetRequiredAsync(model.DrugId ?? 0, "Drug", nameof(model.DrugId));
        if (!drug.IsActive)
        {
            throw new DomainValidationException($"{drug.Name} is discontinued and cannot be stocked.", nameof(model.DrugId));
        }
        if (await _db.WardStocks.AnyAsync(s => s.WardId == ward.Id && s.DrugId == drug.Id))
        {
            throw new DuplicateValueException("Ward stock", nameof(model.DrugId), "drug", $"{drug.Name} on {ward.Code}");
        }

        var initial = model.InitialQuantity ?? 0;
        if (initial < 0)
        {
            throw new DomainValidationException("Opening quantity cannot be negative.", nameof(model.InitialQuantity));
        }

        var now = _clock.Now;
        var stock = new WardStock
        {
            WardId = ward.Id,
            DrugId = drug.Id,
            QuantityOnHand = 0,
            ReorderLevel = Math.Max(model.ReorderLevel ?? 0, 0)
        };
        if (initial > 0)
        {
            stock.Apply(StockMovementType.Opening, initial, now, _currentUser.UserName, "Opening balance");
            stock.LastRestockedAt = now;
        }
        _db.WardStocks.Add(stock);
        await _db.SaveChangesAsync();
        return stock.Id;
    }

    public async Task<WardStockFormViewModel> GetStockForEditAsync(int id)
    {
        var stock = await _db.WardStocks.AsNoTracking().GetRequiredAsync(id, "Ward stock");
        return new WardStockFormViewModel
        {
            Id = stock.Id,
            WardId = stock.WardId,
            DrugId = stock.DrugId,
            InitialQuantity = stock.QuantityOnHand,
            ReorderLevel = stock.ReorderLevel
        };
    }

    public async Task UpdateStockAsync(WardStockFormViewModel model)
    {
        var stock = await _db.WardStocks.GetRequiredAsync(model.Id, "Ward stock");
        if (model.WardId != stock.WardId || model.DrugId != stock.DrugId)
        {
            throw new DomainValidationException("The ward and drug of a stock line cannot be changed.", nameof(model.DrugId));
        }
        if (model.ReorderLevel is null or < 0)
        {
            throw new DomainValidationException("Reorder level must be zero or more.", nameof(model.ReorderLevel));
        }

        stock.ReorderLevel = model.ReorderLevel.Value;
        await _db.SaveChangesAsync();
    }

    public async Task<RestockViewModel> GetRestockFormAsync(int stockId)
    {
        var stock = await _db.WardStocks.AsNoTracking().Include(s => s.Ward).Include(s => s.Drug).GetRequiredAsync(stockId, "Ward stock");
        return new RestockViewModel
        {
            StockId = stock.Id,
            WardName = stock.Ward.Name,
            DrugName = stock.Drug.DisplayName,
            QuantityOnHand = stock.QuantityOnHand
        };
    }

    public async Task RestockAsync(RestockViewModel model)
    {
        var quantity = model.Quantity ?? 0;
        if (quantity <= 0)
        {
            throw new DomainValidationException("Restock quantity must be greater than zero.", nameof(model.Quantity));
        }
        if (string.IsNullOrWhiteSpace(model.Reference))
        {
            throw new DomainValidationException("A delivery reference is required.", nameof(model.Reference));
        }

        var stock = await _db.WardStocks.Include(s => s.Ward).Include(s => s.Drug).GetRequiredAsync(model.StockId, "Ward stock");
        var now = _clock.Now;
        stock.Apply(StockMovementType.Restock, quantity, now, _currentUser.UserName, model.Reference.Trim());
        stock.LastRestockedAt = now;
        _audit.Record(AuditActions.Restock, nameof(WardStock), stock.Id,
            $"+{quantity} {stock.Drug.Name} on {stock.Ward.Code} (ref {model.Reference.Trim()}), now {stock.QuantityOnHand}");
        await SaveStockChangesAsync(stock);
    }

    public async Task<DispenseQueueViewModel> GetDispenseQueueAsync()
    {
        var orders = await _db.ClinicalOrders.AsNoTracking()
            .Include(o => o.Admission).ThenInclude(a => a.Patient)
            .Include(o => o.Admission).ThenInclude(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .Include(o => o.MedicationDetail!).ThenInclude(d => d.Drug)
            .Include(o => o.OrderingPractitioner)
            .Where(o => o.Type == OrderType.Medication
                        && (o.Status == OrderStatus.Placed || o.Status == OrderStatus.InProgress)
                        && o.Admission.Status == AdmissionStatus.Open)
            .OrderByDescending(o => o.Priority).ThenBy(o => o.PlacedAt)
            .ToListAsync();

        var wardIds = orders.Select(o => o.Admission.CurrentBed.WardId).Distinct().ToList();
        var drugIds = orders.Select(o => o.MedicationDetail!.DrugId).Distinct().ToList();
        var stock = await _db.WardStocks.AsNoTracking()
            .Where(s => wardIds.Contains(s.WardId) && drugIds.Contains(s.DrugId))
            .ToListAsync();

        return new DispenseQueueViewModel
        {
            Items = orders.Select(o => new DispenseQueueItem
            {
                Order = o,
                Ward = o.Admission.CurrentBed.Ward,
                QuantityOnHand = stock.FirstOrDefault(s => s.WardId == o.Admission.CurrentBed.WardId && s.DrugId == o.MedicationDetail!.DrugId)?.QuantityOnHand
            }).ToList()
        };
    }

    public async Task<DispenseViewModel> GetDispenseFormAsync(int orderId)
    {
        var order = await LoadMedicationOrderAsync(orderId, tracking: false);
        EnsureDispensable(order);
        var detail = order.MedicationDetail!;
        var ward = order.Admission.CurrentBed.Ward;
        var onHand = await _db.WardStocks
            .Where(s => s.WardId == ward.Id && s.DrugId == detail.DrugId)
            .Select(s => (int?)s.QuantityOnHand)
            .FirstOrDefaultAsync();
        return new DispenseViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            PatientName = order.Admission.Patient.FullName,
            DrugName = detail.Drug.DisplayName,
            WardName = ward.Name,
            Dose = $"{detail.Dose} {detail.Route} {detail.Frequency}",
            Quantity = detail.Quantity,
            QuantityOnHand = onHand
        };
    }

    public async Task DispenseAsync(int orderId, string? notes)
    {
        var order = await LoadMedicationOrderAsync(orderId, tracking: true);
        EnsureDispensable(order);

        var detail = order.MedicationDetail!;
        var ward = order.Admission.CurrentBed.Ward;
        var stock = await _db.WardStocks.FirstOrDefaultAsync(s => s.WardId == ward.Id && s.DrugId == detail.DrugId);
        if (stock == null)
        {
            throw new InsufficientStockException(detail.Drug.Name, ward.Name, 0, detail.Quantity);
        }
        if (stock.QuantityOnHand - detail.Quantity < 0)
        {
            throw new InsufficientStockException(detail.Drug.Name, ward.Name, stock.QuantityOnHand, detail.Quantity);
        }

        var now = _clock.Now;
        var user = _currentUser.UserName;
        stock.Apply(StockMovementType.Dispense, -detail.Quantity, now, user, order.OrderNumber);
        order.Dispense = new DispenseRecord
        {
            WardStock = stock,
            Quantity = detail.Quantity,
            UnitPrice = detail.Drug.UnitPrice,
            DispensedAt = now,
            DispensedBy = user,
            Notes = notes?.Trim()
        };
        order.ChangeStatus(OrderStatus.Completed, now, user, $"Dispensed {detail.Quantity} from {ward.Code}");
        _audit.Record(AuditActions.Dispense, nameof(ClinicalOrder), order.Id,
            $"{order.OrderNumber}: {detail.Quantity} x {detail.Drug.DisplayName} from {ward.Code} for {order.Admission.Patient.MedicalRecordNumber}; {stock.QuantityOnHand} left");
        await SaveStockChangesAsync(stock);
    }

    private async Task SaveStockChangesAsync(WardStock stock)
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainValidationException($"Stock for this item changed while you were working (stock line {stock.Id}). Reload and try again.");
        }
    }

    private async Task<ClinicalOrder> LoadMedicationOrderAsync(int orderId, bool tracking)
    {
        var query = _db.ClinicalOrders
            .Include(o => o.Admission).ThenInclude(a => a.Patient)
            .Include(o => o.Admission).ThenInclude(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .Include(o => o.MedicationDetail!).ThenInclude(d => d.Drug)
            .Include(o => o.Dispense)
            .AsQueryable();
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var order = await query.GetRequiredAsync(orderId, "Order");
        if (order.Type != OrderType.Medication || order.MedicationDetail == null)
        {
            throw new DomainValidationException($"Order {order.OrderNumber} is not a medication order.");
        }
        return order;
    }

    private static void EnsureDispensable(ClinicalOrder order)
    {
        if (order.Status is not (OrderStatus.Placed or OrderStatus.InProgress) || order.Dispense != null)
        {
            throw new InvalidStatusTransitionException("Medication order", order.Status.ToString(), "dispense");
        }
        if (!order.Admission.IsOpen)
        {
            throw new AdmissionClosedException(order.Admission.AdmissionNumber, null);
        }
    }
}
