using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class LabService : ILabService
{
    private static readonly TimeSpan ClockTolerance = TimeSpan.FromMinutes(5);

    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public LabService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<IReadOnlyList<ClinicalOrder>> GetWorklistAsync(OrderStatus? status)
    {
        var query = LabOrders().AsNoTracking();
        query = status.HasValue
            ? query.Where(o => o.Status == status.Value)
            : query.Where(o => o.Status == OrderStatus.Placed || o.Status == OrderStatus.InProgress);

        var orders = await query.OrderBy(o => o.PlacedAt).Take(500).ToListAsync();
        return orders
            .OrderByDescending(o => o.Priority)
            .ThenBy(o => o.PlacedAt)
            .ToList();
    }

    public async Task<LabOrderViewModel> GetOrderAsync(int orderId)
    {
        var order = await LoadLabOrderAsync(orderId, tracking: false);
        return new LabOrderViewModel { Order = order };
    }

    public async Task AcceptAsync(int orderId)
    {
        var order = await LoadLabOrderAsync(orderId, tracking: true);
        if (order.Status != OrderStatus.Placed)
        {
            throw new InvalidStatusTransitionException("Lab order", order.Status.ToString(), "accept");
        }
        if (!order.Admission.IsOpen)
        {
            throw new AdmissionClosedException(order.Admission.AdmissionNumber, null);
        }

        order.ChangeStatus(OrderStatus.InProgress, _clock.Now, _currentUser.UserName, "Accepted by laboratory");
        await _db.SaveChangesAsync();
    }

    public async Task<SpecimenFormViewModel> GetSpecimenFormAsync(int orderId)
    {
        var order = await LoadLabOrderAsync(orderId, tracking: false);
        EnsureInProgress(order, "record a specimen for");
        return new SpecimenFormViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            TestName = order.LabDetail!.TestDefinition.Name,
            PatientName = order.Admission.Patient.FullName,
            SpecimenTypeName = order.LabDetail.TestDefinition.SpecimenType.Name,
            CollectedAt = new DateTime(_clock.Now.Year, _clock.Now.Month, _clock.Now.Day, _clock.Now.Hour, _clock.Now.Minute, 0)
        };
    }

    public async Task RecordSpecimenAsync(SpecimenFormViewModel model)
    {
        var order = await LoadLabOrderAsync(model.OrderId, tracking: true);
        EnsureInProgress(order, "record a specimen for");
        var detail = order.LabDetail!;
        if (detail.Specimen != null)
        {
            throw new DomainValidationException($"Specimen {detail.Specimen.AccessionNumber} has already been recorded for this order.");
        }

        var collectedAt = model.CollectedAt ?? throw new DomainValidationException("Collection time is required.", nameof(model.CollectedAt));
        var now = _clock.Now;
        if (collectedAt > now + ClockTolerance)
        {
            throw new DomainValidationException("Collection time cannot be in the future.", nameof(model.CollectedAt));
        }
        if (collectedAt < order.PlacedAt.AddMinutes(-1))
        {
            throw new DomainValidationException($"Collection time cannot be before the order was placed ({order.PlacedAt:yyyy-MM-dd HH:mm}).", nameof(model.CollectedAt));
        }

        detail.Specimen = new Specimen
        {
            SpecimenTypeId = detail.TestDefinition.SpecimenTypeId,
            AccessionNumber = ReferenceNumbers.Next("ACC", now),
            CollectedAt = collectedAt,
            CollectedBy = _currentUser.UserName,
            Notes = model.Notes?.Trim()
        };
        await _db.SaveChangesAsync();
    }

    public async Task<LabResultFormViewModel> GetResultFormAsync(int orderId)
    {
        var order = await LoadLabOrderAsync(orderId, tracking: false);
        if (order.LabDetail!.Result != null)
        {
            throw new ResultAlreadyRecordedException(order.OrderNumber);
        }
        EnsureInProgress(order, "enter a result for");
        var test = order.LabDetail.TestDefinition;
        return new LabResultFormViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            TestName = test.Name,
            PatientName = order.Admission.Patient.FullName,
            ResultType = test.ResultType,
            ReferenceRange = test.ReferenceRangeDisplay,
            Unit = test.Unit
        };
    }

    public async Task EnterResultAsync(LabResultFormViewModel model)
    {
        var order = await LoadLabOrderAsync(model.OrderId, tracking: true);
        var detail = order.LabDetail!;
        if (detail.Result != null)
        {
            throw new ResultAlreadyRecordedException(order.OrderNumber);
        }
        EnsureInProgress(order, "enter a result for");
        if (detail.Specimen == null)
        {
            throw new DomainValidationException("Record the specimen before entering a result.");
        }

        var test = detail.TestDefinition;
        decimal? numeric = null;
        string? text = null;
        if (test.ResultType == ResultValueType.Numeric)
        {
            numeric = model.NumericValue ?? throw new DomainValidationException($"{test.Name} requires a numeric result.", nameof(model.NumericValue));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.TextValue))
            {
                throw new DomainValidationException($"{test.Name} requires a text result.", nameof(model.TextValue));
            }
            text = model.TextValue.Trim();
        }

        var now = _clock.Now;
        var flag = test.Evaluate(numeric, text);
        detail.Result = new LabResult
        {
            NumericValue = numeric,
            TextValue = text,
            Flag = flag,
            Comment = model.Comment?.Trim(),
            ResultedAt = now,
            ResultedBy = _currentUser.UserName
        };
        order.ChangeStatus(OrderStatus.Completed, now, _currentUser.UserName, $"Result {detail.Result.DisplayValue} ({flag})");
        _audit.Record(AuditActions.ResultEntered, nameof(ClinicalOrder), order.Id, $"{order.OrderNumber} {test.Code} = {detail.Result.DisplayValue} {test.Unit} [{flag}]");
        await _db.SaveChangesAsync();
    }

    private IQueryable<ClinicalOrder> LabOrders() =>
        _db.ClinicalOrders
            .Include(o => o.Admission).ThenInclude(a => a.Patient)
            .Include(o => o.Admission).ThenInclude(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .Include(o => o.OrderingPractitioner)
            .Include(o => o.LabDetail!).ThenInclude(d => d.TestDefinition).ThenInclude(t => t.SpecimenType)
            .Include(o => o.LabDetail!).ThenInclude(d => d.Specimen)
            .Include(o => o.LabDetail!).ThenInclude(d => d.Result)
            .Where(o => o.Type == OrderType.Lab);

    private async Task<ClinicalOrder> LoadLabOrderAsync(int orderId, bool tracking)
    {
        var query = LabOrders();
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var order = await query.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            var exists = await _db.ClinicalOrders.AnyAsync(o => o.Id == orderId);
            if (exists)
            {
                throw new DomainValidationException($"Order {orderId} is not a laboratory order.");
            }
            throw new EntityNotFoundException("Lab order", orderId);
        }
        return order;
    }

    private static void EnsureInProgress(ClinicalOrder order, string action)
    {
        if (order.Status != OrderStatus.InProgress)
        {
            throw new InvalidStatusTransitionException("Lab order", order.Status == OrderStatus.Placed ? "not yet accepted" : order.Status.ToString(), action);
        }
    }
}
