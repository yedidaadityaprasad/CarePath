using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public PatientService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
    }

    public Task<PagedResult<Patient>> SearchAsync(string? search, bool includeInactive, int page, int pageSize = 25)
    {
        var query = _db.Patients.AsNoTracking().AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var mrn = term.ToUpperInvariant();
            query = query.Where(p =>
                p.MedicalRecordNumber.StartsWith(mrn) ||
                p.LastName.Contains(term) ||
                p.FirstName.Contains(term) ||
                (p.FirstName + " " + p.LastName).Contains(term));
        }

        return query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToPagedResultAsync(page, pageSize);
    }

    public async Task<PatientDetailsViewModel> GetDetailsAsync(int id)
    {
        var patient = await _db.Patients.AsNoTracking()
            .Include(p => p.Address)
            .Include(p => p.EmergencyContacts)
            .Include(p => p.Allergies)
            .GetRequiredAsync(id, "Patient");

        var coverages = await _db.InsuranceCoverages.AsNoTracking()
            .Include(c => c.Payer)
            .Where(c => c.PatientId == id)
            .OrderByDescending(c => c.IsActive).ThenByDescending(c => c.EffectiveFrom)
            .ToListAsync();

        var admissions = await _db.Admissions.AsNoTracking()
            .Include(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .Include(a => a.AttendingPractitioner)
            .Where(a => a.PatientId == id)
            .OrderByDescending(a => a.AdmittedAt)
            .ToListAsync();

        var now = _clock.Now;
        var appointments = await _db.Appointments.AsNoTracking()
            .Include(a => a.Practitioner)
            .Where(a => a.PatientId == id && a.Status == AppointmentStatus.Booked && a.StartsAt >= now)
            .OrderBy(a => a.StartsAt)
            .ToListAsync();

        return new PatientDetailsViewModel
        {
            Patient = patient,
            Coverages = coverages,
            Admissions = admissions,
            UpcomingAppointments = appointments
        };
    }

    public async Task<PatientFormViewModel> GetForEditAsync(int id)
    {
        var patient = await _db.Patients.AsNoTracking()
            .Include(p => p.Address)
            .Include(p => p.EmergencyContacts)
            .GetRequiredAsync(id, "Patient");
        var contact = patient.EmergencyContacts.FirstOrDefault(c => c.IsPrimary) ?? patient.EmergencyContacts.FirstOrDefault();

        return new PatientFormViewModel
        {
            Id = patient.Id,
            MedicalRecordNumber = patient.MedicalRecordNumber,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            Phone = patient.Phone,
            Email = patient.Email,
            AddressLine1 = patient.Address?.Line1,
            AddressLine2 = patient.Address?.Line2,
            City = patient.Address?.City,
            State = patient.Address?.State,
            PostalCode = patient.Address?.PostalCode,
            Country = patient.Address?.Country,
            ContactName = contact?.Name,
            ContactRelationship = contact?.Relationship,
            ContactPhone = contact?.Phone
        };
    }

    public async Task<int> RegisterAsync(PatientFormViewModel model)
    {
        ValidateDemographics(model);
        var mrn = NormalizeMrn(model.MedicalRecordNumber);
        if (await _db.Patients.AnyAsync(p => p.MedicalRecordNumber == mrn))
        {
            throw new DuplicateValueException("Patient", nameof(model.MedicalRecordNumber), "medical record number", mrn);
        }

        var patient = new Patient
        {
            MedicalRecordNumber = mrn,
            RegisteredAt = _clock.Now,
            IsActive = true
        };
        ApplyDemographics(patient, model);
        _db.Patients.Add(patient);

        await _db.InTransactionAsync(async () =>
        {
            await _db.SaveChangesAsync();
            _audit.Record(AuditActions.Register, nameof(Patient), patient.Id, $"Registered {mrn} {patient.FullName}");
            await _db.SaveChangesAsync();
        });
        return patient.Id;
    }

    public async Task UpdateAsync(PatientFormViewModel model)
    {
        ValidateDemographics(model);
        var patient = await _db.Patients
            .Include(p => p.Address)
            .Include(p => p.EmergencyContacts)
            .GetRequiredAsync(model.Id, "Patient");

        var mrn = NormalizeMrn(model.MedicalRecordNumber);
        if (!string.Equals(patient.MedicalRecordNumber, mrn, StringComparison.Ordinal))
        {
            throw new DomainValidationException("The medical record number cannot be changed after registration.", nameof(model.MedicalRecordNumber));
        }

        ApplyDemographics(patient, model);
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("A reason is required to deactivate a patient.", "Reason");
        }

        var patient = await _db.Patients.GetRequiredAsync(id, "Patient");
        if (!patient.IsActive)
        {
            throw new InvalidStatusTransitionException("Patient", "already deactivated", "deactivate");
        }

        var openAdmission = await _db.Admissions
            .Where(a => a.PatientId == id && a.Status == AdmissionStatus.Open)
            .Select(a => a.AdmissionNumber)
            .FirstOrDefaultAsync();
        if (openAdmission != null)
        {
            throw new DomainValidationException($"Patient {patient.MedicalRecordNumber} is currently admitted under {openAdmission}. Discharge the patient before deactivating.");
        }

        var now = _clock.Now;
        var futureAppointments = await _db.Appointments
            .Where(a => a.PatientId == id && a.Status == AppointmentStatus.Booked && a.StartsAt > now)
            .ToListAsync();
        foreach (var appointment in futureAppointments)
        {
            appointment.CancelledAt = now;
            appointment.CancellationReason = "Patient deactivated";
            appointment.ChangeStatus(AppointmentStatus.Cancelled, now, _currentUser.UserName, "Patient deactivated");
        }

        patient.IsActive = false;
        patient.DeactivatedAt = now;
        patient.DeactivationReason = reason.Trim();
        _audit.Record(AuditActions.Deactivate, nameof(Patient), patient.Id,
            $"{reason.Trim()} ({futureAppointments.Count} future appointment(s) cancelled)");
        await _db.SaveChangesAsync();
    }

    public async Task ReactivateAsync(int id)
    {
        var patient = await _db.Patients.GetRequiredAsync(id, "Patient");
        if (patient.IsActive)
        {
            throw new InvalidStatusTransitionException("Patient", "already active", "reactivate");
        }

        patient.IsActive = true;
        patient.DeactivatedAt = null;
        patient.DeactivationReason = null;
        _audit.Record(AuditActions.Reactivate, nameof(Patient), patient.Id);
        await _db.SaveChangesAsync();
    }

    public async Task<AllergyFormViewModel> GetAllergyFormAsync(int patientId)
    {
        var patient = await _db.Patients.AsNoTracking().GetRequiredAsync(patientId, "Patient");
        return new AllergyFormViewModel { PatientId = patient.Id, PatientName = patient.FullName };
    }

    public async Task AddAllergyAsync(AllergyFormViewModel model)
    {
        var patient = await _db.Patients.Include(p => p.Allergies).GetRequiredAsync(model.PatientId, "Patient");
        var substance = (model.Substance ?? string.Empty).Trim();
        if (substance.Length == 0)
        {
            throw new DomainValidationException("Substance is required.", nameof(model.Substance));
        }
        if (patient.Allergies.Any(a => string.Equals(a.Substance, substance, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DuplicateValueException("Allergy", nameof(model.Substance), "substance", substance);
        }

        patient.Allergies.Add(new PatientAllergy
        {
            Substance = substance,
            Reaction = model.Reaction?.Trim(),
            Severity = model.Severity ?? AllergySeverity.Moderate,
            RecordedAt = _clock.Now,
            RecordedBy = _currentUser.UserName
        });
        await _db.SaveChangesAsync();
    }

    private static string NormalizeMrn(string? mrn) => (mrn ?? string.Empty).Trim().ToUpperInvariant();

    private void ValidateDemographics(PatientFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.MedicalRecordNumber))
        {
            throw new DomainValidationException("Medical record number is required.", nameof(model.MedicalRecordNumber));
        }
        if (model.DateOfBirth is not { } dob)
        {
            throw new DomainValidationException("Date of birth is required.", nameof(model.DateOfBirth));
        }

        var today = _clock.Now.Date;
        if (dob.Date > today)
        {
            throw new DomainValidationException("Date of birth cannot be in the future.", nameof(model.DateOfBirth));
        }
        if (dob.Date < today.AddYears(-130))
        {
            throw new DomainValidationException("Date of birth is more than 130 years ago.", nameof(model.DateOfBirth));
        }

        var contactFields = new[] { model.ContactName, model.ContactRelationship, model.ContactPhone };
        var provided = contactFields.Count(f => !string.IsNullOrWhiteSpace(f));
        if (provided is > 0 and < 3)
        {
            throw new DomainValidationException("Provide the emergency contact's name, relationship and phone together, or leave all three blank.", nameof(model.ContactName));
        }
    }

    private static void ApplyDemographics(Patient patient, PatientFormViewModel model)
    {
        patient.FirstName = model.FirstName!.Trim();
        patient.LastName = model.LastName!.Trim();
        patient.DateOfBirth = model.DateOfBirth!.Value.Date;
        patient.Gender = model.Gender ?? Gender.Unknown;
        patient.Phone = model.Phone?.Trim();
        patient.Email = model.Email?.Trim();

        patient.Address ??= new PatientAddress();
        patient.Address.Line1 = model.AddressLine1!.Trim();
        patient.Address.Line2 = model.AddressLine2?.Trim();
        patient.Address.City = model.City!.Trim();
        patient.Address.State = model.State?.Trim();
        patient.Address.PostalCode = model.PostalCode!.Trim();
        patient.Address.Country = model.Country!.Trim();

        var primary = patient.EmergencyContacts.FirstOrDefault(c => c.IsPrimary);
        if (string.IsNullOrWhiteSpace(model.ContactName))
        {
            if (primary != null)
            {
                patient.EmergencyContacts.Remove(primary);
            }
            return;
        }

        if (primary == null)
        {
            primary = new EmergencyContact { IsPrimary = true };
            patient.EmergencyContacts.Add(primary);
        }
        primary.Name = model.ContactName.Trim();
        primary.Relationship = model.ContactRelationship!.Trim();
        primary.Phone = model.ContactPhone!.Trim();
    }
}
