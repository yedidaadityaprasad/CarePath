using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class AdmissionService : IAdmissionService
{
    private static readonly TimeSpan ClockTolerance = TimeSpan.FromMinutes(5);

    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;
    private readonly ICoverageService _coverage;
    private readonly IBillingService _billing;

    public AdmissionService(
        ApplicationDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IAuditService audit,
        ICoverageService coverage,
        IBillingService billing)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
        _coverage = coverage;
        _billing = billing;
    }

    public Task<PagedResult<Admission>> SearchAsync(AdmissionStatus? status, int? wardId, string? search, int page, int pageSize = 25)
    {
        var query = _db.Admissions.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.AttendingPractitioner)
            .Include(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }
        if (wardId.HasValue)
        {
            query = query.Where(a => a.CurrentBed.WardId == wardId.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var upper = term.ToUpperInvariant();
            query = query.Where(a =>
                a.AdmissionNumber.StartsWith(upper) ||
                a.Patient.MedicalRecordNumber.StartsWith(upper) ||
                a.Patient.LastName.Contains(term) ||
                a.Patient.FirstName.Contains(term));
        }
        return query.OrderByDescending(a => a.AdmittedAt).ToPagedResultAsync(page, pageSize);
    }

    public async Task<AdmissionDetailsViewModel> GetDetailsAsync(int id)
    {
        var admission = await _db.Admissions.AsNoTracking()
            .Include(a => a.Patient).ThenInclude(p => p.Allergies)
            .Include(a => a.AttendingPractitioner)
            .Include(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .Include(a => a.Coverage).ThenInclude(c => c.Payer)
            .Include(a => a.BedAssignments).ThenInclude(b => b.Bed).ThenInclude(b => b.Ward)
            .Include(a => a.Diagnoses)
            .Include(a => a.Notes)
            .Include(a => a.Vitals)
            .Include(a => a.Orders).ThenInclude(o => o.LabDetail!).ThenInclude(d => d.TestDefinition)
            .Include(a => a.Orders).ThenInclude(o => o.MedicationDetail!).ThenInclude(d => d.Drug)
            .Include(a => a.DischargeSummary)
            .Include(a => a.Invoice)
            .AsSplitQuery()
            .GetRequiredAsync(id, "Admission");

        return new AdmissionDetailsViewModel
        {
            Admission = admission,
            LengthOfStayDays = admission.LengthOfStayDays(_clock.Now)
        };
    }

    public async Task<int> AdmitAsync(AdmitPatientViewModel model)
    {
        var now = _clock.Now;
        var admittedAt = model.AdmittedAt ?? throw new DomainValidationException("Admission time is required.", nameof(model.AdmittedAt));
        if (admittedAt > now + ClockTolerance)
        {
            throw new DomainValidationException("Admission time cannot be in the future.", nameof(model.AdmittedAt));
        }

        var patient = await _db.Patients.GetRequiredAsync(model.PatientId ?? 0, "Patient", nameof(model.PatientId));
        if (!patient.IsActive)
        {
            throw new PatientInactiveException(patient.MedicalRecordNumber, nameof(model.PatientId));
        }

        var existing = await _db.Admissions
            .Where(a => a.PatientId == patient.Id && a.Status == AdmissionStatus.Open)
            .Select(a => a.AdmissionNumber)
            .FirstOrDefaultAsync();
        if (existing != null)
        {
            throw new DomainValidationException($"Patient {patient.MedicalRecordNumber} is already admitted under {existing}.", nameof(model.PatientId));
        }

        var attending = await _db.Practitioners.GetRequiredAsync(model.AttendingPractitionerId ?? 0, "Practitioner", nameof(model.AttendingPractitionerId));
        EnsureAttendingPhysician(attending, nameof(model.AttendingPractitionerId));

        var bed = await _db.Beds.Include(b => b.Ward).GetRequiredAsync(model.BedId ?? 0, "Bed", nameof(model.BedId));
        EnsureAssignable(bed, nameof(model.BedId));

        var coverage = await _coverage.FindActiveCoverageAsync(patient.Id, admittedAt)
            ?? throw new CoverageRequiredException(patient.MedicalRecordNumber, admittedAt, nameof(model.PatientId));

        var user = _currentUser.UserName;
        var admission = new Admission
        {
            AdmissionNumber = ReferenceNumbers.Next("ADM", now),
            PatientId = patient.Id,
            AttendingPractitionerId = attending.Id,
            CurrentBed = bed,
            CoverageId = coverage.Id,
            Status = AdmissionStatus.Open,
            AdmittedAt = admittedAt,
            ReasonForAdmission = model.ReasonForAdmission!.Trim(),
            AdmittedBy = user
        };
        admission.BedAssignments.Add(new BedAssignment
        {
            Bed = bed,
            AssignedAt = admittedAt,
            Reason = "Admission",
            AssignedBy = user
        });
        admission.Diagnoses.Add(new AdmissionDiagnosis
        {
            Code = model.DiagnosisCode!.Trim().ToUpperInvariant(),
            Description = model.DiagnosisDescription!.Trim(),
            IsPrimary = true,
            RecordedAt = now,
            RecordedBy = user
        });
        bed.ChangeStatus(BedStatus.Occupied, now, user, $"Admitted {admission.AdmissionNumber}");
        _db.Admissions.Add(admission);

        await _db.InTransactionAsync(async () =>
        {
            await _db.SaveChangesAsync();
            _audit.Record(AuditActions.Admit, nameof(Admission), admission.Id,
                $"{admission.AdmissionNumber}: {patient.MedicalRecordNumber} to bed {bed.BedLabel()} under {attending.DisplayName}, coverage {coverage.Payer.Code}/{coverage.MemberId}");
            await _db.SaveChangesAsync();
        });
        return admission.Id;
    }

    public async Task<AdmissionEditViewModel> GetForEditAsync(int id)
    {
        var admission = await _db.Admissions.AsNoTracking().Include(a => a.Patient).GetRequiredAsync(id, "Admission");
        EnsureOpen(admission);
        return new AdmissionEditViewModel
        {
            Id = admission.Id,
            AdmissionNumber = admission.AdmissionNumber,
            PatientName = admission.Patient.FullName,
            AttendingPractitionerId = admission.AttendingPractitionerId,
            ReasonForAdmission = admission.ReasonForAdmission
        };
    }

    public async Task UpdateAsync(AdmissionEditViewModel model)
    {
        var admission = await _db.Admissions.GetRequiredAsync(model.Id, "Admission");
        EnsureOpen(admission);

        if (model.AttendingPractitionerId != admission.AttendingPractitionerId)
        {
            var attending = await _db.Practitioners.GetRequiredAsync(model.AttendingPractitionerId ?? 0, "Practitioner", nameof(model.AttendingPractitionerId));
            EnsureAttendingPhysician(attending, nameof(model.AttendingPractitionerId));
            admission.AttendingPractitionerId = attending.Id;
        }

        admission.ReasonForAdmission = (model.ReasonForAdmission ?? string.Empty).Trim();
        await _db.SaveChangesAsync();
    }

    public async Task<TransferViewModel> GetTransferFormAsync(int id)
    {
        var admission = await _db.Admissions.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .GetRequiredAsync(id, "Admission");
        EnsureOpen(admission);
        return new TransferViewModel
        {
            AdmissionId = admission.Id,
            AdmissionNumber = admission.AdmissionNumber,
            PatientName = admission.Patient.FullName,
            CurrentBed = $"{admission.CurrentBed.BedLabel()} ({admission.CurrentBed.Ward.Name})"
        };
    }

    public async Task TransferAsync(TransferViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Reason))
        {
            throw new DomainValidationException("A reason for the transfer is required.", nameof(model.Reason));
        }

        var admission = await _db.Admissions
            .Include(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .Include(a => a.BedAssignments)
            .GetRequiredAsync(model.AdmissionId, "Admission");
        EnsureOpen(admission);

        if (model.NewBedId == admission.CurrentBedId)
        {
            throw new DomainValidationException("The patient is already in that bed.", nameof(model.NewBedId));
        }

        var newBed = await _db.Beds.Include(b => b.Ward).GetRequiredAsync(model.NewBedId ?? 0, "Bed", nameof(model.NewBedId));
        EnsureAssignable(newBed, nameof(model.NewBedId));

        var now = _clock.Now;
        var user = _currentUser.UserName;
        var oldBed = admission.CurrentBed;
        var reason = model.Reason.Trim();

        foreach (var open in admission.BedAssignments.Where(b => b.ReleasedAt == null))
        {
            open.ReleasedAt = now;
        }
        oldBed.ChangeStatus(BedStatus.Available, now, user, $"Transfer out {admission.AdmissionNumber}");
        newBed.ChangeStatus(BedStatus.Occupied, now, user, $"Transfer in {admission.AdmissionNumber}");
        admission.BedAssignments.Add(new BedAssignment
        {
            Bed = newBed,
            AssignedAt = now,
            Reason = reason,
            AssignedBy = user
        });
        admission.CurrentBed = newBed;
        admission.CurrentBedId = newBed.Id;

        _audit.Record(AuditActions.Transfer, nameof(Admission), admission.Id,
            $"{admission.AdmissionNumber}: {oldBed.BedLabel()} to {newBed.BedLabel()} - {reason}");
        await _db.SaveChangesAsync();
    }

    public async Task<DischargeViewModel> GetDischargeFormAsync(int id)
    {
        var admission = await _db.Admissions.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Diagnoses)
            .GetRequiredAsync(id, "Admission");
        if (!admission.IsOpen)
        {
            throw new InvalidStatusTransitionException("Admission", "already discharged", "discharge");
        }

        var primary = admission.Diagnoses.FirstOrDefault(d => d.IsPrimary);
        return new DischargeViewModel
        {
            AdmissionId = admission.Id,
            AdmissionNumber = admission.AdmissionNumber,
            PatientName = admission.Patient.FullName,
            AdmittedAt = admission.AdmittedAt,
            DischargedAt = TrimToMinute(_clock.Now),
            FinalDiagnosis = primary == null ? null : $"{primary.Code} {primary.Description}"
        };
    }

    public async Task<int> DischargeAsync(DischargeViewModel model)
    {
        var admission = await _db.Admissions
            .Include(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .Include(a => a.BedAssignments)
            .Include(a => a.Orders)
            .Include(a => a.Patient)
            .GetRequiredAsync(model.AdmissionId, "Admission");

        if (admission.Status == AdmissionStatus.Discharged)
        {
            throw new InvalidStatusTransitionException("Admission", "already discharged", "discharge");
        }

        var now = _clock.Now;
        var dischargedAt = model.DischargedAt ?? now;
        if (dischargedAt < admission.AdmittedAt)
        {
            throw new DomainValidationException("Discharge time cannot be before the admission time.", nameof(model.DischargedAt));
        }
        if (dischargedAt > now + ClockTolerance)
        {
            throw new DomainValidationException("Discharge time cannot be in the future.", nameof(model.DischargedAt));
        }

        var inProgress = admission.Orders.Where(o => o.Status == OrderStatus.InProgress).Select(o => o.OrderNumber).ToList();
        if (inProgress.Count > 0)
        {
            throw new DomainValidationException($"Orders still in progress must be completed before discharge: {string.Join(", ", inProgress)}.");
        }

        var user = _currentUser.UserName;
        var cancelled = 0;
        foreach (var order in admission.Orders.Where(o => o.Status == OrderStatus.Placed))
        {
            order.ChangeStatus(OrderStatus.Cancelled, now, user, "Cancelled automatically on discharge");
            cancelled++;
        }

        foreach (var open in admission.BedAssignments.Where(b => b.ReleasedAt == null))
        {
            open.ReleasedAt = dischargedAt;
        }
        admission.CurrentBed.ChangeStatus(BedStatus.Available, now, user, $"Discharged {admission.AdmissionNumber}");
        admission.Status = AdmissionStatus.Discharged;
        admission.DischargedAt = dischargedAt;
        admission.DischargeSummary = new DischargeSummary
        {
            Disposition = (model.Disposition ?? string.Empty).Trim(),
            FinalDiagnosis = (model.FinalDiagnosis ?? string.Empty).Trim(),
            Summary = (model.Summary ?? string.Empty).Trim(),
            FollowUpInstructions = model.FollowUpInstructions?.Trim(),
            WrittenAt = now,
            WrittenBy = user
        };

        _audit.Record(AuditActions.Discharge, nameof(Admission), admission.Id,
            $"{admission.AdmissionNumber}: {admission.Patient.MedicalRecordNumber} discharged from {admission.CurrentBed.BedLabel()} ({model.Disposition}); {cancelled} pending order(s) cancelled");

        return await _db.InTransactionAsync(async () =>
        {
            await _db.SaveChangesAsync();
            return await _billing.CreateInvoiceForDischargeAsync(admission.Id);
        });
    }

    public async Task<AdmissionNoteViewModel> GetNoteFormAsync(int id)
    {
        var admission = await LoadOpenWithPatientAsync(id);
        return new AdmissionNoteViewModel
        {
            AdmissionId = admission.Id,
            AdmissionNumber = admission.AdmissionNumber,
            PatientName = admission.Patient.FullName
        };
    }

    public async Task AddNoteAsync(AdmissionNoteViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Text))
        {
            throw new DomainValidationException("The note cannot be empty.", nameof(model.Text));
        }

        var admission = await _db.Admissions.GetRequiredAsync(model.AdmissionId, "Admission");
        EnsureOpen(admission);
        _db.AdmissionNotes.Add(new AdmissionNote
        {
            AdmissionId = admission.Id,
            Text = model.Text.Trim(),
            WrittenAt = _clock.Now,
            Author = _currentUser.UserName
        });
        await _db.SaveChangesAsync();
    }

    public async Task<VitalsViewModel> GetVitalsFormAsync(int id)
    {
        var admission = await LoadOpenWithPatientAsync(id);
        return new VitalsViewModel
        {
            AdmissionId = admission.Id,
            AdmissionNumber = admission.AdmissionNumber,
            PatientName = admission.Patient.FullName
        };
    }

    public async Task RecordVitalsAsync(VitalsViewModel model)
    {
        if (model.SystolicBp.HasValue && model.DiastolicBp.HasValue && model.DiastolicBp >= model.SystolicBp)
        {
            throw new DomainValidationException("Diastolic pressure must be lower than systolic pressure.", nameof(model.DiastolicBp));
        }

        var admission = await _db.Admissions.GetRequiredAsync(model.AdmissionId, "Admission");
        EnsureOpen(admission);
        _db.VitalSigns.Add(new VitalSign
        {
            AdmissionId = admission.Id,
            RecordedAt = _clock.Now,
            TemperatureCelsius = model.TemperatureCelsius ?? throw new DomainValidationException("Temperature is required.", nameof(model.TemperatureCelsius)),
            PulseBpm = model.PulseBpm ?? throw new DomainValidationException("Pulse is required.", nameof(model.PulseBpm)),
            SystolicBp = model.SystolicBp ?? throw new DomainValidationException("Systolic pressure is required.", nameof(model.SystolicBp)),
            DiastolicBp = model.DiastolicBp ?? throw new DomainValidationException("Diastolic pressure is required.", nameof(model.DiastolicBp)),
            RespiratoryRate = model.RespiratoryRate ?? throw new DomainValidationException("Respiratory rate is required.", nameof(model.RespiratoryRate)),
            OxygenSaturation = model.OxygenSaturation ?? throw new DomainValidationException("SpO2 is required.", nameof(model.OxygenSaturation)),
            RecordedBy = _currentUser.UserName
        });
        await _db.SaveChangesAsync();
    }

    private async Task<Admission> LoadOpenWithPatientAsync(int id)
    {
        var admission = await _db.Admissions.AsNoTracking().Include(a => a.Patient).GetRequiredAsync(id, "Admission");
        EnsureOpen(admission);
        return admission;
    }

    private static void EnsureOpen(Admission admission)
    {
        if (!admission.IsOpen)
        {
            throw new AdmissionClosedException(admission.AdmissionNumber, null);
        }
    }

    private static void EnsureAttendingPhysician(Practitioner practitioner, string propertyName)
    {
        if (!practitioner.IsActive)
        {
            throw new PractitionerInactiveException(practitioner.DisplayName, propertyName);
        }
        if (practitioner.Type != PractitionerType.Physician)
        {
            throw new DomainValidationException($"{practitioner.DisplayName} is not a physician and cannot be the attending practitioner.", propertyName);
        }
    }

    private static void EnsureAssignable(Bed bed, string propertyName)
    {
        if (bed.Ward.IsClosed)
        {
            throw new WardClosedException(bed.Ward.Name, propertyName);
        }
        if (bed.Status != BedStatus.Available)
        {
            throw new BedUnavailableException(bed.BedLabel(), bed.Status, propertyName);
        }
    }

    private static DateTime TrimToMinute(DateTime value) =>
        new(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind);
}
