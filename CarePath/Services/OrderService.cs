using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public OrderService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
    }

    public Task<PagedResult<ClinicalOrder>> SearchAsync(OrderType? type, OrderStatus? status, string? search, int page, int pageSize = 25)
    {
        var query = _db.ClinicalOrders.AsNoTracking()
            .Include(o => o.Admission).ThenInclude(a => a.Patient)
            .Include(o => o.OrderingPractitioner)
            .Include(o => o.LabDetail!).ThenInclude(d => d.TestDefinition)
            .Include(o => o.MedicationDetail!).ThenInclude(d => d.Drug)
            .AsQueryable();
        if (type.HasValue)
        {
            query = query.Where(o => o.Type == type.Value);
        }
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var upper = term.ToUpperInvariant();
            query = query.Where(o =>
                o.OrderNumber.StartsWith(upper) ||
                o.Admission.AdmissionNumber.StartsWith(upper) ||
                o.Admission.Patient.MedicalRecordNumber.StartsWith(upper) ||
                o.Admission.Patient.LastName.Contains(term));
        }
        return query.OrderByDescending(o => o.PlacedAt).ToPagedResultAsync(page, pageSize);
    }

    public async Task<OrderDetailsViewModel> GetDetailsAsync(int id)
    {
        var order = await _db.ClinicalOrders.AsNoTracking()
            .Include(o => o.Admission).ThenInclude(a => a.Patient)
            .Include(o => o.Admission).ThenInclude(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .Include(o => o.OrderingPractitioner)
            .Include(o => o.LabDetail!).ThenInclude(d => d.TestDefinition)
            .Include(o => o.LabDetail!).ThenInclude(d => d.Specimen)
            .Include(o => o.LabDetail!).ThenInclude(d => d.Result)
            .Include(o => o.MedicationDetail!).ThenInclude(d => d.Drug)
            .Include(o => o.Dispense)
            .Include(o => o.StatusChanges)
            .AsSplitQuery()
            .GetRequiredAsync(id, "Order");
        return new OrderDetailsViewModel { Order = order };
    }

    public async Task<int> PlaceLabOrderAsync(LabOrderFormViewModel model)
    {
        var admission = await LoadOpenAdmissionAsync(model.AdmissionId);
        var practitioner = await LoadActivePractitionerAsync(model.OrderingPractitionerId);
        var test = await _db.LabTestDefinitions.GetRequiredAsync(model.LabTestDefinitionId ?? 0, "Lab test", nameof(model.LabTestDefinitionId));
        if (!test.IsActive)
        {
            throw new DomainValidationException($"Lab test {test.Name} has been retired and cannot be ordered.", nameof(model.LabTestDefinitionId));
        }

        var duplicate = await _db.ClinicalOrders.AnyAsync(o =>
            o.AdmissionId == admission.Id &&
            o.Type == OrderType.Lab &&
            (o.Status == OrderStatus.Placed || o.Status == OrderStatus.InProgress) &&
            o.LabDetail!.LabTestDefinitionId == test.Id);
        if (duplicate)
        {
            throw new DomainValidationException($"{test.Name} is already pending for this admission.", nameof(model.LabTestDefinitionId));
        }

        var order = NewOrder(admission, practitioner, OrderType.Lab, model.Priority, model.Notes);
        order.LabDetail = new LabOrderDetail
        {
            LabTestDefinitionId = test.Id,
            ClinicalIndication = model.ClinicalIndication?.Trim()
        };
        return await SaveNewOrderAsync(order, $"Lab {test.Code} for {admission.AdmissionNumber}");
    }

    public async Task<int> PlaceMedicationOrderAsync(MedicationOrderFormViewModel model)
    {
        var admission = await LoadOpenAdmissionAsync(model.AdmissionId);
        var practitioner = await LoadActivePractitionerAsync(model.OrderingPractitionerId);
        var drug = await _db.Drugs.GetRequiredAsync(model.DrugId ?? 0, "Drug", nameof(model.DrugId));
        if (!drug.IsActive)
        {
            throw new DomainValidationException($"{drug.Name} has been discontinued and cannot be ordered.", nameof(model.DrugId));
        }
        if (model.Quantity is null or < 1)
        {
            throw new DomainValidationException("Quantity must be at least 1.", nameof(model.Quantity));
        }

        var allergies = await _db.PatientAllergies.Where(a => a.PatientId == admission.PatientId).ToListAsync();
        var allergy = allergies.FirstOrDefault(a => drug.Name.Contains(a.Substance, StringComparison.OrdinalIgnoreCase));
        if (allergy != null)
        {
            throw new DomainValidationException($"The patient has a recorded {allergy.Severity.ToString().ToLowerInvariant()} allergy to {allergy.Substance}.", nameof(model.DrugId));
        }

        var order = NewOrder(admission, practitioner, OrderType.Medication, model.Priority, model.Notes);
        order.MedicationDetail = new MedicationOrderDetail
        {
            DrugId = drug.Id,
            Dose = Required(model.Dose, nameof(model.Dose)),
            Route = Required(model.Route, nameof(model.Route)),
            Frequency = Required(model.Frequency, nameof(model.Frequency)),
            Quantity = model.Quantity.Value
        };
        return await SaveNewOrderAsync(order, $"{drug.DisplayName} x{model.Quantity} for {admission.AdmissionNumber}");
    }

    public async Task<OrderEditViewModel> GetForEditAsync(int id)
    {
        var order = await _db.ClinicalOrders.AsNoTracking()
            .Include(o => o.LabDetail!).ThenInclude(d => d.TestDefinition)
            .Include(o => o.MedicationDetail!).ThenInclude(d => d.Drug)
            .GetRequiredAsync(id, "Order");
        EnsurePlaced(order, "edit");
        return new OrderEditViewModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Type = order.Type,
            Description = order.Description,
            Priority = order.Priority,
            Notes = order.Notes,
            Dose = order.MedicationDetail?.Dose,
            Route = order.MedicationDetail?.Route,
            Frequency = order.MedicationDetail?.Frequency,
            Quantity = order.MedicationDetail?.Quantity
        };
    }

    public async Task UpdateAsync(OrderEditViewModel model)
    {
        var order = await _db.ClinicalOrders
            .Include(o => o.MedicationDetail)
            .Include(o => o.Admission)
            .GetRequiredAsync(model.Id, "Order");
        EnsurePlaced(order, "edit");
        if (!order.Admission.IsOpen)
        {
            throw new AdmissionClosedException(order.Admission.AdmissionNumber, null);
        }

        order.Priority = model.Priority ?? order.Priority;
        order.Notes = model.Notes?.Trim();
        if (order.Type == OrderType.Medication && order.MedicationDetail != null)
        {
            if (model.Quantity is null or < 1)
            {
                throw new DomainValidationException("Quantity must be at least 1.", nameof(model.Quantity));
            }
            order.MedicationDetail.Dose = Required(model.Dose, nameof(model.Dose));
            order.MedicationDetail.Route = Required(model.Route, nameof(model.Route));
            order.MedicationDetail.Frequency = Required(model.Frequency, nameof(model.Frequency));
            order.MedicationDetail.Quantity = model.Quantity.Value;
        }
        await _db.SaveChangesAsync();
    }

    public async Task CancelAsync(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("A reason is required to cancel an order.", "Reason");
        }

        var order = await _db.ClinicalOrders.GetRequiredAsync(id, "Order");
        EnsurePlaced(order, "cancel");
        order.ChangeStatus(OrderStatus.Cancelled, _clock.Now, _currentUser.UserName, reason.Trim());
        _audit.Record(AuditActions.CancelOrder, nameof(ClinicalOrder), order.Id, $"{order.OrderNumber}: {reason.Trim()}");
        await _db.SaveChangesAsync();
    }

    private ClinicalOrder NewOrder(Admission admission, Practitioner practitioner, OrderType type, OrderPriority? priority, string? notes)
    {
        var now = _clock.Now;
        return new ClinicalOrder
        {
            OrderNumber = ReferenceNumbers.Next("ORD", now),
            AdmissionId = admission.Id,
            OrderingPractitionerId = practitioner.Id,
            Type = type,
            Priority = priority ?? OrderPriority.Routine,
            Status = OrderStatus.Placed,
            Notes = notes?.Trim(),
            PlacedAt = now,
            PlacedBy = _currentUser.UserName
        };
    }

    private async Task<int> SaveNewOrderAsync(ClinicalOrder order, string details)
    {
        _db.ClinicalOrders.Add(order);
        await _db.InTransactionAsync(async () =>
        {
            await _db.SaveChangesAsync();
            _audit.Record(AuditActions.PlaceOrder, nameof(ClinicalOrder), order.Id, $"{order.OrderNumber}: {details}");
            await _db.SaveChangesAsync();
        });
        return order.Id;
    }

    private async Task<Admission> LoadOpenAdmissionAsync(int? admissionId)
    {
        var admission = await _db.Admissions.GetRequiredAsync(admissionId ?? 0, "Admission", "AdmissionId");
        if (!admission.IsOpen)
        {
            throw new AdmissionClosedException(admission.AdmissionNumber);
        }
        return admission;
    }

    private async Task<Practitioner> LoadActivePractitionerAsync(int? practitionerId)
    {
        var practitioner = await _db.Practitioners.GetRequiredAsync(practitionerId ?? 0, "Practitioner", "OrderingPractitionerId");
        if (!practitioner.IsActive)
        {
            throw new PractitionerInactiveException(practitioner.DisplayName, "OrderingPractitionerId");
        }
        return practitioner;
    }

    private static void EnsurePlaced(ClinicalOrder order, string action)
    {
        if (order.Status != OrderStatus.Placed)
        {
            throw new InvalidStatusTransitionException("Order", order.Status == OrderStatus.InProgress ? "already started" : order.Status.ToString(), action);
        }
    }

    private static string Required(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"{propertyName} is required.", propertyName);
        }
        return value.Trim();
    }
}
