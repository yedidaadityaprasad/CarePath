using CarePath.Data;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class LookupService : ILookupService
{
    private static readonly string[] DispositionOptions =
    {
        "Discharged home",
        "Discharged home with services",
        "Transferred to another facility",
        "Left against medical advice",
        "Deceased"
    };

    private readonly ApplicationDbContext _db;

    public LookupService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SelectListItem>> PatientsAsync(bool activeOnly = true)
    {
        var query = _db.Patients.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }
        return await query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Select(p => new SelectListItem($"{p.LastName}, {p.FirstName} ({p.MedicalRecordNumber})", p.Id.ToString()))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SelectListItem>> PractitionersAsync(PractitionerType? type = null, bool activeOnly = true)
    {
        var query = _db.Practitioners.AsNoTracking();
        if (type.HasValue)
        {
            query = query.Where(p => p.Type == type.Value);
        }
        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }
        var practitioners = await query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToListAsync();
        return practitioners
            .Select(p => new SelectListItem($"{p.DisplayName} ({p.LicenseNumber})", p.Id.ToString()))
            .ToList();
    }

    public async Task<IReadOnlyList<SelectListItem>> AvailableBedsAsync()
    {
        return await _db.Beds.AsNoTracking()
            .Where(b => b.Status == BedStatus.Available && !b.Ward.IsClosed)
            .OrderBy(b => b.Ward.Code).ThenBy(b => b.BedNumber)
            .Select(b => new SelectListItem($"{b.Ward.Code}-{b.BedNumber} ({b.Ward.Name}, {b.Ward.DailyRate:0.00}/day)", b.Id.ToString()))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SelectListItem>> WardsAsync(bool openOnly = false)
    {
        var query = _db.Wards.AsNoTracking();
        if (openOnly)
        {
            query = query.Where(w => !w.IsClosed);
        }
        return await query.OrderBy(w => w.Code)
            .Select(w => new SelectListItem(w.IsClosed ? $"{w.Code} - {w.Name} (closed)" : $"{w.Code} - {w.Name}", w.Id.ToString()))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SelectListItem>> PayersAsync(bool activeOnly = true)
    {
        var query = _db.Payers.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }
        return await query.OrderBy(p => p.Name)
            .Select(p => new SelectListItem($"{p.Name} ({p.Code})", p.Id.ToString()))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SelectListItem>> SpecialtiesAsync() =>
        await _db.Specialties.AsNoTracking().OrderBy(s => s.Name)
            .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
            .ToListAsync();

    public async Task<IReadOnlyList<SelectListItem>> DepartmentsAsync() =>
        await _db.Departments.AsNoTracking().OrderBy(d => d.Name)
            .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
            .ToListAsync();

    public async Task<IReadOnlyList<SelectListItem>> SpecimenTypesAsync() =>
        await _db.SpecimenTypes.AsNoTracking().OrderBy(s => s.Name)
            .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
            .ToListAsync();

    public async Task<IReadOnlyList<SelectListItem>> DrugCategoriesAsync() =>
        await _db.DrugCategories.AsNoTracking().OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();

    public async Task<IReadOnlyList<SelectListItem>> DrugsAsync(bool activeOnly = true)
    {
        var query = _db.Drugs.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(d => d.IsActive);
        }
        return await query.OrderBy(d => d.Name).ThenBy(d => d.Strength)
            .Select(d => new SelectListItem($"{d.Name} {d.Strength} {d.Form} ({d.Code})", d.Id.ToString()))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SelectListItem>> LabTestsAsync(bool activeOnly = true)
    {
        var query = _db.LabTestDefinitions.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }
        return await query.OrderBy(t => t.Name)
            .Select(t => new SelectListItem($"{t.Name} ({t.Code})", t.Id.ToString()))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SelectListItem>> OpenAdmissionsAsync() =>
        await _db.Admissions.AsNoTracking()
            .Where(a => a.Status == AdmissionStatus.Open)
            .OrderBy(a => a.Patient.LastName)
            .Select(a => new SelectListItem(
                $"{a.AdmissionNumber} - {a.Patient.LastName}, {a.Patient.FirstName} ({a.CurrentBed.Ward.Code}-{a.CurrentBed.BedNumber})",
                a.Id.ToString()))
            .ToListAsync();

    public IReadOnlyList<SelectListItem> Dispositions() =>
        DispositionOptions.Select(d => new SelectListItem(d, d)).ToList();

    public IReadOnlyList<SelectListItem> Roles() =>
        Infrastructure.Roles.All.Select(r => new SelectListItem(r, r)).ToList();
}
