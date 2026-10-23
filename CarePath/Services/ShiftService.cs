using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class ShiftService : IShiftService
{
    public static readonly TimeSpan MaximumShiftLength = TimeSpan.FromHours(16);

    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public ShiftService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<IReadOnlyList<Shift>> SearchAsync(int? wardId, int? practitionerId, DateTime? from, DateTime? to, bool includeCancelled)
    {
        var start = (from ?? _clock.Now.Date).Date;
        var endExclusive = (to ?? start.AddDays(14)).Date.AddDays(1);
        if (endExclusive <= start)
        {
            throw new DomainValidationException("The 'To' date must be on or after the 'From' date.", "To");
        }

        var query = _db.Shifts.AsNoTracking()
            .Include(s => s.Practitioner)
            .Include(s => s.Ward)
            .Where(s => s.StartsAt < endExclusive && s.EndsAt > start);
        if (wardId.HasValue)
        {
            query = query.Where(s => s.WardId == wardId.Value);
        }
        if (practitionerId.HasValue)
        {
            query = query.Where(s => s.PractitionerId == practitionerId.Value);
        }
        if (!includeCancelled)
        {
            query = query.Where(s => s.Status == ShiftStatus.Scheduled);
        }
        return await query.OrderBy(s => s.StartsAt).ThenBy(s => s.Ward.Code).ToListAsync();
    }

    public async Task<ShiftDetailsViewModel> GetDetailsAsync(int id)
    {
        var shift = await _db.Shifts.AsNoTracking()
            .Include(s => s.Practitioner)
            .Include(s => s.Ward)
            .GetRequiredAsync(id, "Shift");
        var now = _clock.Now;
        return new ShiftDetailsViewModel
        {
            Shift = shift,
            CanEdit = shift.Status == ShiftStatus.Scheduled && shift.StartsAt > now,
            CanCancel = shift.Status == ShiftStatus.Scheduled && shift.EndsAt > now
        };
    }

    public async Task<ShiftFormViewModel> GetForEditAsync(int id)
    {
        var shift = await _db.Shifts.AsNoTracking().GetRequiredAsync(id, "Shift");
        EnsureEditable(shift);
        return new ShiftFormViewModel
        {
            Id = shift.Id,
            PractitionerId = shift.PractitionerId,
            WardId = shift.WardId,
            StartsAt = shift.StartsAt,
            EndsAt = shift.EndsAt,
            Notes = shift.Notes
        };
    }

    public async Task<int> ScheduleAsync(ShiftFormViewModel model)
    {
        var (practitioner, ward, start, end) = await ValidateAsync(model, null);
        var shift = new Shift
        {
            PractitionerId = practitioner.Id,
            WardId = ward.Id,
            StartsAt = start,
            EndsAt = end,
            Notes = model.Notes?.Trim(),
            Status = ShiftStatus.Scheduled,
            CreatedAt = _clock.Now,
            CreatedBy = _currentUser.UserName
        };
        _db.Shifts.Add(shift);

        await _db.InTransactionAsync(async () =>
        {
            await _db.SaveChangesAsync();
            _audit.Record(AuditActions.ScheduleShift, nameof(Shift), shift.Id,
                $"{practitioner.DisplayName} on {ward.Code} {start:yyyy-MM-dd HH:mm}-{end:yyyy-MM-dd HH:mm}");
            await _db.SaveChangesAsync();
        });
        return shift.Id;
    }

    public async Task UpdateAsync(ShiftFormViewModel model)
    {
        var shift = await _db.Shifts.GetRequiredAsync(model.Id, "Shift");
        EnsureEditable(shift);
        var (practitioner, ward, start, end) = await ValidateAsync(model, shift.Id);

        shift.PractitionerId = practitioner.Id;
        shift.WardId = ward.Id;
        shift.StartsAt = start;
        shift.EndsAt = end;
        shift.Notes = model.Notes?.Trim();
        await _db.SaveChangesAsync();
    }

    public async Task CancelAsync(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("A reason is required to cancel a shift.", "Reason");
        }

        var shift = await _db.Shifts.GetRequiredAsync(id, "Shift");
        if (shift.Status == ShiftStatus.Cancelled)
        {
            throw new InvalidStatusTransitionException("Shift", "already cancelled", "cancel");
        }
        var now = _clock.Now;
        if (shift.EndsAt <= now)
        {
            throw new InvalidStatusTransitionException("Shift", "already finished", "cancel");
        }

        shift.Status = ShiftStatus.Cancelled;
        shift.CancelledAt = now;
        shift.CancellationReason = reason.Trim();
        _audit.Record(AuditActions.CancelShift, nameof(Shift), shift.Id, reason.Trim());
        await _db.SaveChangesAsync();
    }

    private void EnsureEditable(Shift shift)
    {
        if (shift.Status == ShiftStatus.Cancelled)
        {
            throw new InvalidStatusTransitionException("Shift", "cancelled", "edit");
        }
        if (shift.StartsAt <= _clock.Now)
        {
            throw new InvalidStatusTransitionException("Shift", "already started", "edit");
        }
    }

    private async Task<(Practitioner Practitioner, Ward Ward, DateTime Start, DateTime End)> ValidateAsync(ShiftFormViewModel model, int? excludeShiftId)
    {
        var start = model.StartsAt ?? throw new DomainValidationException("Start time is required.", nameof(model.StartsAt));
        var end = model.EndsAt ?? throw new DomainValidationException("End time is required.", nameof(model.EndsAt));
        if (end <= start)
        {
            throw new DomainValidationException("The shift must end after it starts.", nameof(model.EndsAt));
        }
        if (end - start > MaximumShiftLength)
        {
            throw new DomainValidationException($"A shift cannot be longer than {MaximumShiftLength.TotalHours:0} hours.", nameof(model.EndsAt));
        }
        if (end <= _clock.Now)
        {
            throw new DomainValidationException("A shift cannot be scheduled entirely in the past.", nameof(model.EndsAt));
        }

        var practitioner = await _db.Practitioners.GetRequiredAsync(model.PractitionerId ?? 0, "Practitioner", nameof(model.PractitionerId));
        if (!practitioner.IsActive)
        {
            throw new PractitionerInactiveException(practitioner.DisplayName, nameof(model.PractitionerId));
        }

        var ward = await _db.Wards.GetRequiredAsync(model.WardId ?? 0, "Ward", nameof(model.WardId));
        if (ward.IsClosed)
        {
            throw new WardClosedException(ward.Name, nameof(model.WardId));
        }

        var conflict = await _db.Shifts
            .Include(s => s.Ward)
            .Where(s => s.PractitionerId == practitioner.Id
                        && s.Status == ShiftStatus.Scheduled
                        && (excludeShiftId == null || s.Id != excludeShiftId)
                        && s.StartsAt < end
                        && start < s.EndsAt)
            .OrderBy(s => s.StartsAt)
            .FirstOrDefaultAsync();
        if (conflict != null)
        {
            throw new SchedulingConflictException(
                $"{practitioner.DisplayName} is already scheduled on {conflict.Ward.Name} from {conflict.StartsAt:yyyy-MM-dd HH:mm} to {conflict.EndsAt:yyyy-MM-dd HH:mm}.",
                nameof(model.StartsAt));
        }

        return (practitioner, ward, start, end);
    }
}
