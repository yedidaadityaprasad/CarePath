using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class WardService : IWardService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public WardService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<IReadOnlyList<WardSummary>> ListWardsAsync(bool includeClosed)
    {
        var query = _db.Wards.AsNoTracking().Include(w => w.Department).AsQueryable();
        if (!includeClosed)
        {
            query = query.Where(w => !w.IsClosed);
        }

        var wards = await query.OrderBy(w => w.Code).ToListAsync();
        var wardIds = wards.Select(w => w.Id).ToList();
        var bedCounts = await _db.Beds.AsNoTracking()
            .Where(b => wardIds.Contains(b.WardId))
            .GroupBy(b => new { b.WardId, b.Status })
            .Select(g => new { g.Key.WardId, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        return wards.Select(w =>
        {
            int Count(BedStatus status) => bedCounts.Where(c => c.WardId == w.Id && c.Status == status).Sum(c => c.Count);
            var occupied = Count(BedStatus.Occupied);
            var available = Count(BedStatus.Available);
            var outOfService = Count(BedStatus.OutOfService);
            return new WardSummary
            {
                Ward = w,
                Occupied = occupied,
                Available = available,
                OutOfService = outOfService,
                TotalBeds = occupied + available + outOfService
            };
        }).ToList();
    }

    public async Task<WardDetailsViewModel> GetWardDetailsAsync(int id)
    {
        var ward = await _db.Wards.AsNoTracking().Include(w => w.Department).GetRequiredAsync(id, "Ward");
        var beds = await _db.Beds.AsNoTracking().Where(b => b.WardId == id).OrderBy(b => b.BedNumber).ToListAsync();
        var occupants = await _db.Admissions.AsNoTracking()
            .Include(a => a.Patient)
            .Where(a => a.Status == AdmissionStatus.Open && a.CurrentBed.WardId == id)
            .ToListAsync();

        return new WardDetailsViewModel
        {
            Ward = ward,
            Beds = beds,
            OccupantsByBed = occupants.ToDictionary(a => a.CurrentBedId)
        };
    }

    public async Task<WardFormViewModel> GetWardForEditAsync(int id)
    {
        var ward = await _db.Wards.AsNoTracking().GetRequiredAsync(id, "Ward");
        return new WardFormViewModel
        {
            Id = ward.Id,
            Code = ward.Code,
            Name = ward.Name,
            DepartmentId = ward.DepartmentId,
            Floor = ward.Floor,
            DailyRate = ward.DailyRate
        };
    }

    public async Task<int> CreateWardAsync(WardFormViewModel model)
    {
        var code = await ValidateWardAsync(model, null);
        var ward = new Ward { Code = code };
        ApplyWard(ward, model);
        _db.Wards.Add(ward);
        await _db.SaveChangesAsync();
        return ward.Id;
    }

    public async Task UpdateWardAsync(WardFormViewModel model)
    {
        var ward = await _db.Wards.GetRequiredAsync(model.Id, "Ward");
        ward.Code = await ValidateWardAsync(model, ward.Id);
        ApplyWard(ward, model);
        await _db.SaveChangesAsync();
    }

    public async Task CloseWardAsync(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("A reason is required to close a ward.", "Reason");
        }

        var ward = await _db.Wards.GetRequiredAsync(id, "Ward");
        if (ward.IsClosed)
        {
            throw new InvalidStatusTransitionException("Ward", "already closed", "close");
        }

        var occupied = await _db.Beds.CountAsync(b => b.WardId == id && b.Status == BedStatus.Occupied);
        if (occupied > 0)
        {
            throw new DomainValidationException($"Ward {ward.Name} still has {occupied} occupied bed(s). Transfer or discharge those patients first.");
        }

        ward.IsClosed = true;
        ward.ClosedAt = _clock.Now;
        ward.ClosureReason = reason.Trim();
        _audit.Record(AuditActions.CloseWard, nameof(Ward), ward.Id, reason.Trim());
        await _db.SaveChangesAsync();
    }

    public async Task ReopenWardAsync(int id)
    {
        var ward = await _db.Wards.GetRequiredAsync(id, "Ward");
        if (!ward.IsClosed)
        {
            throw new InvalidStatusTransitionException("Ward", "already open", "reopen");
        }

        ward.IsClosed = false;
        ward.ClosedAt = null;
        ward.ClosureReason = null;
        _audit.Record(AuditActions.ReopenWard, nameof(Ward), ward.Id);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Bed>> ListBedsAsync(int? wardId, BedStatus? status)
    {
        var query = _db.Beds.AsNoTracking().Include(b => b.Ward).AsQueryable();
        if (wardId.HasValue)
        {
            query = query.Where(b => b.WardId == wardId.Value);
        }
        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }
        return await query.OrderBy(b => b.Ward.Code).ThenBy(b => b.BedNumber).ToListAsync();
    }

    public async Task<BedDetailsViewModel> GetBedDetailsAsync(int id)
    {
        var bed = await _db.Beds.AsNoTracking().Include(b => b.Ward).GetRequiredAsync(id, "Bed");
        var history = await _db.BedStatusChanges.AsNoTracking()
            .Where(c => c.BedId == id)
            .OrderByDescending(c => c.ChangedAt).ThenByDescending(c => c.Id)
            .Take(50)
            .ToListAsync();
        var current = await _db.Admissions.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.AttendingPractitioner)
            .FirstOrDefaultAsync(a => a.CurrentBedId == id && a.Status == AdmissionStatus.Open);
        return new BedDetailsViewModel { Bed = bed, History = history, CurrentAdmission = current };
    }

    public async Task<BedFormViewModel> GetBedForEditAsync(int id)
    {
        var bed = await _db.Beds.AsNoTracking().GetRequiredAsync(id, "Bed");
        return new BedFormViewModel { Id = bed.Id, WardId = bed.WardId, BedNumber = bed.BedNumber, Notes = bed.Notes };
    }

    public async Task<int> AddBedAsync(BedFormViewModel model)
    {
        var ward = await _db.Wards.GetRequiredAsync(model.WardId ?? 0, "Ward", nameof(model.WardId));
        var number = NormalizeBedNumber(model.BedNumber);
        await EnsureBedNumberUniqueAsync(ward.Id, number, null);

        var bed = new Bed
        {
            WardId = ward.Id,
            BedNumber = number,
            Notes = model.Notes?.Trim(),
            Status = BedStatus.Available
        };
        _db.Beds.Add(bed);
        await _db.SaveChangesAsync();
        return bed.Id;
    }

    public async Task UpdateBedAsync(BedFormViewModel model)
    {
        var bed = await _db.Beds.GetRequiredAsync(model.Id, "Bed");
        var ward = await _db.Wards.GetRequiredAsync(model.WardId ?? 0, "Ward", nameof(model.WardId));
        var number = NormalizeBedNumber(model.BedNumber);

        if (ward.Id != bed.WardId && bed.Status == BedStatus.Occupied)
        {
            throw new DomainValidationException("An occupied bed cannot be moved to another ward. Transfer the patient first.", nameof(model.WardId));
        }
        await EnsureBedNumberUniqueAsync(ward.Id, number, bed.Id);

        bed.WardId = ward.Id;
        bed.BedNumber = number;
        bed.Notes = model.Notes?.Trim();
        await _db.SaveChangesAsync();
    }

    public async Task SetBedOutOfServiceAsync(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("A reason is required.", "Reason");
        }

        var bed = await _db.Beds.Include(b => b.Ward).GetRequiredAsync(id, "Bed");
        if (bed.Status != BedStatus.Available)
        {
            throw new InvalidStatusTransitionException("Bed", bed.Status.ToString(), "take out of service");
        }

        bed.ChangeStatus(BedStatus.OutOfService, _clock.Now, _currentUser.UserName, reason.Trim());
        _audit.Record(AuditActions.BedOutOfService, nameof(Bed), bed.Id, $"{bed.BedLabel()}: {reason.Trim()}");
        await _db.SaveChangesAsync();
    }

    public async Task ReturnBedToServiceAsync(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("A reason is required.", "Reason");
        }

        var bed = await _db.Beds.Include(b => b.Ward).GetRequiredAsync(id, "Bed");
        if (bed.Status != BedStatus.OutOfService)
        {
            throw new InvalidStatusTransitionException("Bed", bed.Status.ToString(), "return to service");
        }

        bed.ChangeStatus(BedStatus.Available, _clock.Now, _currentUser.UserName, reason.Trim());
        _audit.Record(AuditActions.BedReturnedToService, nameof(Bed), bed.Id, $"{bed.BedLabel()}: {reason.Trim()}");
        await _db.SaveChangesAsync();
    }

    private async Task<string> ValidateWardAsync(WardFormViewModel model, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(model.Code))
        {
            throw new DomainValidationException("Code is required.", nameof(model.Code));
        }
        if (model.DailyRate is null or < 0)
        {
            throw new DomainValidationException("Daily rate must be zero or more.", nameof(model.DailyRate));
        }

        var code = model.Code.Trim().ToUpperInvariant();
        if (await _db.Wards.AnyAsync(w => w.Code == code && w.Id != excludeId))
        {
            throw new DuplicateValueException("Ward", nameof(model.Code), "code", code);
        }
        if (!await _db.Departments.AnyAsync(d => d.Id == model.DepartmentId))
        {
            throw new EntityNotFoundException("Department", model.DepartmentId ?? 0, nameof(model.DepartmentId));
        }
        return code;
    }

    private static void ApplyWard(Ward ward, WardFormViewModel model)
    {
        ward.Name = (model.Name ?? string.Empty).Trim();
        ward.DepartmentId = model.DepartmentId!.Value;
        ward.Floor = model.Floor ?? 0;
        ward.DailyRate = model.DailyRate!.Value;
    }

    private static string NormalizeBedNumber(string? bedNumber)
    {
        if (string.IsNullOrWhiteSpace(bedNumber))
        {
            throw new DomainValidationException("Bed number is required.", nameof(BedFormViewModel.BedNumber));
        }
        return bedNumber.Trim().ToUpperInvariant();
    }

    private async Task EnsureBedNumberUniqueAsync(int wardId, string number, int? excludeId)
    {
        if (await _db.Beds.AnyAsync(b => b.WardId == wardId && b.BedNumber == number && b.Id != excludeId))
        {
            throw new DuplicateValueException("Bed", nameof(BedFormViewModel.BedNumber), "number", number);
        }
    }
}
