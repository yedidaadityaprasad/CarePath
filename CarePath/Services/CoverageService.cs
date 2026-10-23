using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class CoverageService : ICoverageService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditService _audit;

    public CoverageService(ApplicationDbContext db, IClock clock, IAuditService audit)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
    }

    public Task<PagedResult<InsuranceCoverage>> SearchAsync(string? search, bool activeOnly, int page, int pageSize = 25)
    {
        var query = _db.InsuranceCoverages.AsNoTracking()
            .Include(c => c.Patient)
            .Include(c => c.Payer)
            .AsQueryable();
        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var upper = term.ToUpperInvariant();
            query = query.Where(c =>
                c.MemberId.Contains(term) ||
                c.Patient.MedicalRecordNumber.StartsWith(upper) ||
                c.Patient.LastName.Contains(term) ||
                c.Patient.FirstName.Contains(term));
        }

        return query.OrderBy(c => c.Patient.LastName).ThenByDescending(c => c.EffectiveFrom).ToPagedResultAsync(page, pageSize);
    }

    public async Task<CoverageDetailsViewModel> GetDetailsAsync(int id)
    {
        var coverage = await _db.InsuranceCoverages.AsNoTracking()
            .Include(c => c.Patient)
            .Include(c => c.Payer)
            .GetRequiredAsync(id, "Coverage");
        var admissions = await _db.Admissions.CountAsync(a => a.CoverageId == id);
        return new CoverageDetailsViewModel { Coverage = coverage, AdmissionCount = admissions };
    }

    public async Task<CoverageFormViewModel> GetForEditAsync(int id)
    {
        var coverage = await _db.InsuranceCoverages.AsNoTracking()
            .Include(c => c.Patient)
            .GetRequiredAsync(id, "Coverage");
        return new CoverageFormViewModel
        {
            Id = coverage.Id,
            PatientId = coverage.PatientId,
            PatientName = $"{coverage.Patient.FullName} ({coverage.Patient.MedicalRecordNumber})",
            PayerId = coverage.PayerId,
            MemberId = coverage.MemberId,
            GroupNumber = coverage.GroupNumber,
            EffectiveFrom = coverage.EffectiveFrom,
            EffectiveTo = coverage.EffectiveTo,
            CoveragePercent = coverage.CoveragePercent
        };
    }

    public async Task<int> AttachAsync(CoverageFormViewModel model)
    {
        var (from, to) = ValidateDates(model);
        var patient = await _db.Patients.GetRequiredAsync(model.PatientId ?? 0, "Patient", nameof(model.PatientId));
        if (!patient.IsActive)
        {
            throw new PatientInactiveException(patient.MedicalRecordNumber, nameof(model.PatientId));
        }

        var payer = await _db.Payers.GetRequiredAsync(model.PayerId ?? 0, "Payer", nameof(model.PayerId));
        if (!payer.IsActive)
        {
            throw new DomainValidationException($"Payer {payer.Name} is inactive and cannot be attached.", nameof(model.PayerId));
        }

        await EnsureNoOverlapAsync(patient.Id, payer, from, to, excludeCoverageId: null);

        var coverage = new InsuranceCoverage
        {
            PatientId = patient.Id,
            PayerId = payer.Id,
            MemberId = model.MemberId!.Trim(),
            GroupNumber = model.GroupNumber?.Trim(),
            EffectiveFrom = from,
            EffectiveTo = to,
            CoveragePercent = model.CoveragePercent ?? payer.DefaultCoveragePercent,
            IsActive = true,
            CreatedAt = _clock.Now
        };
        _db.InsuranceCoverages.Add(coverage);

        await _db.InTransactionAsync(async () =>
        {
            await _db.SaveChangesAsync();
            _audit.Record(AuditActions.AttachCoverage, nameof(InsuranceCoverage), coverage.Id,
                $"{payer.Code} member {coverage.MemberId} for {patient.MedicalRecordNumber}");
            await _db.SaveChangesAsync();
        });
        return coverage.Id;
    }

    public async Task UpdateAsync(CoverageFormViewModel model)
    {
        var (from, to) = ValidateDates(model);
        var coverage = await _db.InsuranceCoverages.Include(c => c.Payer).GetRequiredAsync(model.Id, "Coverage");
        if (!coverage.IsActive)
        {
            throw new InvalidStatusTransitionException("Coverage", "terminated", "edit");
        }
        if (model.PayerId != coverage.PayerId)
        {
            throw new DomainValidationException("The payer cannot be changed. Terminate this coverage and attach a new one instead.", nameof(model.PayerId));
        }

        await EnsureNoOverlapAsync(coverage.PatientId, coverage.Payer, from, to, coverage.Id);

        var openAdmission = await _db.Admissions
            .Where(a => a.CoverageId == coverage.Id && a.Status == AdmissionStatus.Open)
            .Select(a => new { a.AdmissionNumber, a.AdmittedAt })
            .FirstOrDefaultAsync();
        if (openAdmission != null && (from > openAdmission.AdmittedAt.Date || (to.HasValue && to.Value < openAdmission.AdmittedAt.Date)))
        {
            throw new DomainValidationException(
                $"Open admission {openAdmission.AdmissionNumber} relies on this coverage from {openAdmission.AdmittedAt:yyyy-MM-dd}; the new dates would no longer cover it.",
                nameof(model.EffectiveFrom));
        }

        coverage.MemberId = model.MemberId!.Trim();
        coverage.GroupNumber = model.GroupNumber?.Trim();
        coverage.EffectiveFrom = from;
        coverage.EffectiveTo = to;
        coverage.CoveragePercent = model.CoveragePercent ?? coverage.Payer.DefaultCoveragePercent;
        await _db.SaveChangesAsync();
    }

    public async Task TerminateAsync(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("A reason is required to terminate coverage.", "Reason");
        }

        var coverage = await _db.InsuranceCoverages.GetRequiredAsync(id, "Coverage");
        if (!coverage.IsActive)
        {
            throw new InvalidStatusTransitionException("Coverage", "already terminated", "terminate");
        }

        var openAdmission = await _db.Admissions
            .Where(a => a.CoverageId == id && a.Status == AdmissionStatus.Open)
            .Select(a => a.AdmissionNumber)
            .FirstOrDefaultAsync();
        if (openAdmission != null)
        {
            throw new DomainValidationException($"Coverage is in use by open admission {openAdmission} and cannot be terminated until discharge.");
        }

        var today = _clock.Now.Date;
        coverage.IsActive = false;
        coverage.TerminatedAt = _clock.Now;
        coverage.TerminationReason = reason.Trim();
        if (coverage.EffectiveTo == null || coverage.EffectiveTo.Value > today)
        {
            coverage.EffectiveTo = today < coverage.EffectiveFrom ? coverage.EffectiveFrom : today;
        }
        _audit.Record(AuditActions.TerminateCoverage, nameof(InsuranceCoverage), coverage.Id, reason.Trim());
        await _db.SaveChangesAsync();
    }

    public async Task<InsuranceCoverage?> FindActiveCoverageAsync(int patientId, DateTime onDate)
    {
        var date = onDate.Date;
        return await _db.InsuranceCoverages
            .Include(c => c.Payer)
            .Where(c => c.PatientId == patientId
                        && c.IsActive
                        && c.Payer.IsActive
                        && c.EffectiveFrom <= date
                        && (c.EffectiveTo == null || c.EffectiveTo >= date))
            .OrderByDescending(c => c.CoveragePercent)
            .ThenBy(c => c.Id)
            .FirstOrDefaultAsync();
    }

    private static (DateTime From, DateTime? To) ValidateDates(CoverageFormViewModel model)
    {
        if (model.EffectiveFrom is not { } fromValue)
        {
            throw new DomainValidationException("Effective from date is required.", nameof(model.EffectiveFrom));
        }
        if (string.IsNullOrWhiteSpace(model.MemberId))
        {
            throw new DomainValidationException("Member id is required.", nameof(model.MemberId));
        }

        var from = fromValue.Date;
        var to = model.EffectiveTo?.Date;
        if (to.HasValue && to.Value < from)
        {
            throw new DomainValidationException("Effective to date must be on or after the effective from date.", nameof(model.EffectiveTo));
        }
        if (model.CoveragePercent is < 0 or > 100)
        {
            throw new DomainValidationException("Coverage percent must be between 0 and 100.", nameof(model.CoveragePercent));
        }
        return (from, to);
    }

    private async Task EnsureNoOverlapAsync(int patientId, Payer payer, DateTime from, DateTime? to, int? excludeCoverageId)
    {
        var samePayer = await _db.InsuranceCoverages
            .Where(c => c.PatientId == patientId && c.PayerId == payer.Id && c.IsActive)
            .Where(c => excludeCoverageId == null || c.Id != excludeCoverageId)
            .ToListAsync();
        var conflict = samePayer.FirstOrDefault(c => c.Overlaps(from, to));
        if (conflict != null)
        {
            throw new CoverageConflictException(payer.Name, conflict.EffectiveFrom, conflict.EffectiveTo);
        }
    }
}
