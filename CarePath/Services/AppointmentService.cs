using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public AppointmentService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<Appointment>> SearchAsync(DateTime? from, DateTime? to, int? practitionerId, AppointmentStatus? status)
    {
        var start = (from ?? _clock.Now.Date).Date;
        var endExclusive = (to ?? start.AddDays(30)).Date.AddDays(1);
        if (endExclusive <= start)
        {
            throw new DomainValidationException("The 'To' date must be on or after the 'From' date.", "To");
        }

        var query = _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Practitioner)
            .Where(a => a.StartsAt >= start && a.StartsAt < endExclusive);
        if (practitionerId.HasValue)
        {
            query = query.Where(a => a.PractitionerId == practitionerId.Value);
        }
        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }
        return await query.OrderBy(a => a.StartsAt).ToListAsync();
    }

    public async Task<AppointmentDetailsViewModel> GetDetailsAsync(int id)
    {
        var appointment = await _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Practitioner)
            .Include(a => a.StatusChanges)
            .GetRequiredAsync(id, "Appointment");
        return new AppointmentDetailsViewModel
        {
            Appointment = appointment,
            Complete = new CompleteAppointmentViewModel { Id = appointment.Id }
        };
    }

    public async Task<AppointmentFormViewModel> GetForEditAsync(int id)
    {
        var appointment = await _db.Appointments.AsNoTracking().GetRequiredAsync(id, "Appointment");
        EnsureBooked(appointment, "reschedule");
        return new AppointmentFormViewModel
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PractitionerId = appointment.PractitionerId,
            StartsAt = appointment.StartsAt,
            DurationMinutes = appointment.DurationMinutes,
            Reason = appointment.Reason,
            Location = appointment.Location
        };
    }

    public async Task<int> BookAsync(AppointmentFormViewModel model)
    {
        var (patient, practitioner, start, end) = await ValidateAsync(model, null);
        var now = _clock.Now;
        var appointment = new Appointment
        {
            PatientId = patient.Id,
            PractitionerId = practitioner.Id,
            StartsAt = start,
            EndsAt = end,
            Reason = model.Reason!.Trim(),
            Location = model.Location?.Trim(),
            Status = AppointmentStatus.Booked,
            BookedAt = now,
            BookedBy = _currentUser.UserName
        };
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return appointment.Id;
    }

    public async Task RescheduleAsync(AppointmentFormViewModel model)
    {
        var appointment = await _db.Appointments.GetRequiredAsync(model.Id, "Appointment");
        EnsureBooked(appointment, "reschedule");
        if (model.PatientId != appointment.PatientId)
        {
            throw new DomainValidationException("The patient on an appointment cannot be changed. Cancel it and book a new one.", nameof(model.PatientId));
        }

        var (_, practitioner, start, end) = await ValidateAsync(model, appointment.Id);
        var previous = $"{appointment.StartsAt:yyyy-MM-dd HH:mm}";
        appointment.PractitionerId = practitioner.Id;
        appointment.StartsAt = start;
        appointment.EndsAt = end;
        appointment.Reason = model.Reason!.Trim();
        appointment.Location = model.Location?.Trim();
        appointment.StatusChanges.Add(new AppointmentStatusChange
        {
            FromStatus = AppointmentStatus.Booked,
            ToStatus = AppointmentStatus.Booked,
            ChangedAt = _clock.Now,
            ChangedBy = _currentUser.UserName,
            Note = $"Rescheduled from {previous} to {start:yyyy-MM-dd HH:mm}"
        });
        await _db.SaveChangesAsync();
    }

    public async Task CancelAsync(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("A reason is required to cancel an appointment.", "Reason");
        }

        var appointment = await _db.Appointments.GetRequiredAsync(id, "Appointment");
        EnsureBooked(appointment, "cancel");

        var now = _clock.Now;
        appointment.CancelledAt = now;
        appointment.CancellationReason = reason.Trim();
        appointment.ChangeStatus(AppointmentStatus.Cancelled, now, _currentUser.UserName, reason.Trim());
        await _db.SaveChangesAsync();
    }

    public async Task CompleteAsync(int id, string? notes)
    {
        var appointment = await _db.Appointments.GetRequiredAsync(id, "Appointment");
        EnsureBooked(appointment, "complete");

        var now = _clock.Now;
        if (appointment.StartsAt > now)
        {
            throw new DomainValidationException($"This appointment starts at {appointment.StartsAt:yyyy-MM-dd HH:mm} and cannot be completed before then.");
        }

        appointment.CompletedAt = now;
        appointment.CompletionNotes = notes?.Trim();
        appointment.ChangeStatus(AppointmentStatus.Completed, now, _currentUser.UserName, notes?.Trim());
        await _db.SaveChangesAsync();
    }

    private static void EnsureBooked(Appointment appointment, string action)
    {
        if (appointment.Status != AppointmentStatus.Booked)
        {
            throw new InvalidStatusTransitionException("Appointment", appointment.Status.ToString(), action);
        }
    }

    private async Task<(Patient Patient, Practitioner Practitioner, DateTime Start, DateTime End)> ValidateAsync(AppointmentFormViewModel model, int? excludeId)
    {
        var start = model.StartsAt ?? throw new DomainValidationException("Start time is required.", nameof(model.StartsAt));
        var minutes = model.DurationMinutes ?? 0;
        if (minutes is < 5 or > 240)
        {
            throw new DomainValidationException("Duration must be between 5 and 240 minutes.", nameof(model.DurationMinutes));
        }
        if (start <= _clock.Now)
        {
            throw new DomainValidationException("Appointments must be booked in a future slot.", nameof(model.StartsAt));
        }
        if (string.IsNullOrWhiteSpace(model.Reason))
        {
            throw new DomainValidationException("A reason is required.", nameof(model.Reason));
        }
        var end = start.AddMinutes(minutes);

        var patient = await _db.Patients.GetRequiredAsync(model.PatientId ?? 0, "Patient", nameof(model.PatientId));
        if (!patient.IsActive)
        {
            throw new PatientInactiveException(patient.MedicalRecordNumber, nameof(model.PatientId));
        }

        var practitioner = await _db.Practitioners.GetRequiredAsync(model.PractitionerId ?? 0, "Practitioner", nameof(model.PractitionerId));
        if (!practitioner.IsActive)
        {
            throw new PractitionerInactiveException(practitioner.DisplayName, nameof(model.PractitionerId));
        }

        var clash = await _db.Appointments
            .Where(a => a.PractitionerId == practitioner.Id
                        && a.Status != AppointmentStatus.Cancelled
                        && (excludeId == null || a.Id != excludeId)
                        && a.StartsAt < end
                        && start < a.EndsAt)
            .OrderBy(a => a.StartsAt)
            .FirstOrDefaultAsync();
        if (clash != null)
        {
            throw new SchedulingConflictException(
                $"{practitioner.DisplayName} is already booked from {clash.StartsAt:yyyy-MM-dd HH:mm} to {clash.EndsAt:HH:mm}.",
                nameof(model.StartsAt));
        }

        var patientClash = await _db.Appointments
            .AnyAsync(a => a.PatientId == patient.Id
                           && a.Status == AppointmentStatus.Booked
                           && (excludeId == null || a.Id != excludeId)
                           && a.StartsAt < end
                           && start < a.EndsAt);
        if (patientClash)
        {
            throw new SchedulingConflictException("The patient already has another appointment at that time.", nameof(model.StartsAt));
        }

        return (patient, practitioner, start, end);
    }
}
