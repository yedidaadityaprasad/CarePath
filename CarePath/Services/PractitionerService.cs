using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class PractitionerService : IPractitionerService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public PractitionerService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<IReadOnlyList<Practitioner>> SearchAsync(string? search, PractitionerType? type, bool activeOnly)
    {
        var query = _db.Practitioners.AsNoTracking().Include(p => p.Specialty).AsQueryable();
        if (type.HasValue)
        {
            query = query.Where(p => p.Type == type.Value);
        }
        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var upper = term.ToUpperInvariant();
            query = query.Where(p => p.LicenseNumber.StartsWith(upper) || p.LastName.Contains(term) || p.FirstName.Contains(term));
        }
        return await query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToListAsync();
    }

    public async Task<PractitionerDetailsViewModel> GetDetailsAsync(int id)
    {
        var practitioner = await _db.Practitioners.AsNoTracking()
            .Include(p => p.Specialty)
            .Include(p => p.StatusChanges)
            .GetRequiredAsync(id, "Practitioner");
        var now = _clock.Now;

        var shifts = await _db.Shifts.AsNoTracking()
            .Include(s => s.Ward)
            .Where(s => s.PractitionerId == id && s.Status == ShiftStatus.Scheduled && s.EndsAt >= now)
            .OrderBy(s => s.StartsAt)
            .Take(20)
            .ToListAsync();
        var appointments = await _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Where(a => a.PractitionerId == id && a.Status == AppointmentStatus.Booked && a.StartsAt >= now)
            .OrderBy(a => a.StartsAt)
            .Take(20)
            .ToListAsync();
        var openAdmissions = await _db.Admissions.CountAsync(a => a.AttendingPractitionerId == id && a.Status == AdmissionStatus.Open);

        return new PractitionerDetailsViewModel
        {
            Practitioner = practitioner,
            UpcomingShifts = shifts,
            UpcomingAppointments = appointments,
            OpenAdmissionCount = openAdmissions
        };
    }

    public async Task<PractitionerFormViewModel> GetForEditAsync(int id)
    {
        var p = await _db.Practitioners.AsNoTracking().GetRequiredAsync(id, "Practitioner");
        return new PractitionerFormViewModel
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Type = p.Type,
            LicenseNumber = p.LicenseNumber,
            SpecialtyId = p.SpecialtyId,
            Email = p.Email,
            Phone = p.Phone
        };
    }

    public async Task<int> CreateAsync(PractitionerFormViewModel model)
    {
        var license = NormalizeLicense(model.LicenseNumber);
        await EnsureLicenseUniqueAsync(license, null);
        await EnsureSpecialtyExistsAsync(model.SpecialtyId);

        var practitioner = new Practitioner
        {
            LicenseNumber = license,
            IsActive = true,
            CreatedAt = _clock.Now
        };
        Apply(practitioner, model);
        _db.Practitioners.Add(practitioner);
        await _db.SaveChangesAsync();
        return practitioner.Id;
    }

    public async Task UpdateAsync(PractitionerFormViewModel model)
    {
        var practitioner = await _db.Practitioners.GetRequiredAsync(model.Id, "Practitioner");
        var license = NormalizeLicense(model.LicenseNumber);
        await EnsureLicenseUniqueAsync(license, practitioner.Id);
        await EnsureSpecialtyExistsAsync(model.SpecialtyId);

        if (model.Type != practitioner.Type && practitioner.Type == PractitionerType.Physician)
        {
            var attending = await _db.Admissions.CountAsync(a => a.AttendingPractitionerId == practitioner.Id && a.Status == AdmissionStatus.Open);
            if (attending > 0)
            {
                throw new DomainValidationException($"This physician is attending {attending} open admission(s); reassign them before changing the practitioner type.", nameof(model.Type));
            }
        }

        practitioner.LicenseNumber = license;
        Apply(practitioner, model);
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id, string reason)
    {
        RequireReason(reason);
        var practitioner = await _db.Practitioners.GetRequiredAsync(id, "Practitioner");
        if (!practitioner.IsActive)
        {
            throw new InvalidStatusTransitionException("Practitioner", "already inactive", "deactivate");
        }

        var attending = await _db.Admissions.CountAsync(a => a.AttendingPractitionerId == id && a.Status == AdmissionStatus.Open);
        if (attending > 0)
        {
            throw new DomainValidationException($"{practitioner.DisplayName} is attending {attending} open admission(s). Reassign them before deactivating.");
        }

        var now = _clock.Now;
        var futureShifts = await _db.Shifts
            .Where(s => s.PractitionerId == id && s.Status == ShiftStatus.Scheduled && s.StartsAt > now)
            .ToListAsync();
        foreach (var shift in futureShifts)
        {
            shift.Status = ShiftStatus.Cancelled;
            shift.CancelledAt = now;
            shift.CancellationReason = "Practitioner deactivated";
        }

        var futureAppointments = await _db.Appointments
            .Where(a => a.PractitionerId == id && a.Status == AppointmentStatus.Booked && a.StartsAt > now)
            .ToListAsync();
        foreach (var appointment in futureAppointments)
        {
            appointment.CancelledAt = now;
            appointment.CancellationReason = "Practitioner deactivated";
            appointment.ChangeStatus(AppointmentStatus.Cancelled, now, _currentUser.UserName, "Practitioner deactivated");
        }

        practitioner.IsActive = false;
        AddStatusChange(practitioner, false, reason);
        _audit.Record(AuditActions.Deactivate, nameof(Practitioner), practitioner.Id,
            $"{reason.Trim()} ({futureShifts.Count} shift(s) and {futureAppointments.Count} appointment(s) cancelled)");
        await _db.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id, string reason)
    {
        RequireReason(reason);
        var practitioner = await _db.Practitioners.GetRequiredAsync(id, "Practitioner");
        if (practitioner.IsActive)
        {
            throw new InvalidStatusTransitionException("Practitioner", "already active", "activate");
        }

        practitioner.IsActive = true;
        AddStatusChange(practitioner, true, reason);
        _audit.Record(AuditActions.Reactivate, nameof(Practitioner), practitioner.Id, reason.Trim());
        await _db.SaveChangesAsync();
    }

    private void AddStatusChange(Practitioner practitioner, bool active, string reason)
    {
        _db.PractitionerStatusChanges.Add(new PractitionerStatusChange
        {
            PractitionerId = practitioner.Id,
            IsActive = active,
            Reason = reason.Trim(),
            ChangedAt = _clock.Now,
            ChangedBy = _currentUser.UserName
        });
    }

    private static void RequireReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("A reason is required.", "Reason");
        }
    }

    private static string NormalizeLicense(string? license)
    {
        if (string.IsNullOrWhiteSpace(license))
        {
            throw new DomainValidationException("License number is required.", nameof(PractitionerFormViewModel.LicenseNumber));
        }
        return license.Trim().ToUpperInvariant();
    }

    private async Task EnsureLicenseUniqueAsync(string license, int? excludeId)
    {
        if (await _db.Practitioners.AnyAsync(p => p.LicenseNumber == license && p.Id != excludeId))
        {
            throw new DuplicateValueException("Practitioner", nameof(PractitionerFormViewModel.LicenseNumber), "license number", license);
        }
    }

    private async Task EnsureSpecialtyExistsAsync(int? specialtyId)
    {
        if (specialtyId.HasValue && !await _db.Specialties.AnyAsync(s => s.Id == specialtyId.Value))
        {
            throw new EntityNotFoundException("Specialty", specialtyId.Value, nameof(PractitionerFormViewModel.SpecialtyId));
        }
    }

    private static void Apply(Practitioner practitioner, PractitionerFormViewModel model)
    {
        practitioner.FirstName = (model.FirstName ?? string.Empty).Trim();
        practitioner.LastName = (model.LastName ?? string.Empty).Trim();
        practitioner.Type = model.Type ?? PractitionerType.Physician;
        practitioner.SpecialtyId = model.SpecialtyId;
        practitioner.Email = model.Email?.Trim();
        practitioner.Phone = model.Phone?.Trim();
    }
}
