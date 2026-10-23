using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.Clinical)]
public class AdmissionsController : CarePathController
{
    private readonly IAdmissionService _admissions;
    private readonly ILookupService _lookups;

    public AdmissionsController(IAdmissionService admissions, ILookupService lookups)
    {
        _admissions = admissions;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index(AdmissionStatus? status = AdmissionStatus.Open, int? wardId = null, string? search = null, int page = 1) =>
        View(new AdmissionIndexViewModel
        {
            Status = status,
            WardId = wardId,
            Search = search,
            Page = page,
            Results = await _admissions.SearchAsync(status, wardId, search, page),
            Wards = await _lookups.WardsAsync()
        });

    public async Task<IActionResult> Details(int id) => View(await _admissions.GetDetailsAsync(id));

    [HttpGet]
    public async Task<IActionResult> Create(int? patientId)
    {
        var now = DateTime.Now;
        var model = new AdmitPatientViewModel
        {
            PatientId = patientId,
            AdmittedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0)
        };
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdmitPatientViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model);
            return View(model);
        }
        try
        {
            var id = await _admissions.AdmitAsync(model);
            Success("Patient admitted.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DomainException ex)
        {
            AddDomainError(ex, model);
            await PopulateAsync(model);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _admissions.GetForEditAsync(id);
        model.Physicians = await _lookups.PractitionersAsync(PractitionerType.Physician);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdmissionEditViewModel model)
    {
        model.Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _admissions.UpdateAsync(model);
                Success("Admission updated.");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DomainException ex) when (ex is not EntityNotFoundException)
            {
                AddDomainError(ex, model);
            }
        }
        model.Physicians = await _lookups.PractitionersAsync(PractitionerType.Physician);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Transfer(int id)
    {
        var model = await _admissions.GetTransferFormAsync(id);
        model.Beds = await _lookups.AvailableBedsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(int id, TransferViewModel model)
    {
        model.AdmissionId = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _admissions.TransferAsync(model);
                Success("Patient transferred.");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DomainException ex) when (ex is not EntityNotFoundException)
            {
                AddDomainError(ex, model);
            }
        }
        model.Beds = await _lookups.AvailableBedsAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Discharge(int id)
    {
        var model = await _admissions.GetDischargeFormAsync(id);
        model.Dispositions = _lookups.Dispositions();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Discharge(int id, DischargeViewModel model)
    {
        model.AdmissionId = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _admissions.DischargeAsync(model);
                Success("Patient discharged. A draft invoice has been created for billing.");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DomainException ex) when (ex is not EntityNotFoundException)
            {
                AddDomainError(ex, model);
            }
        }
        model.Dispositions = _lookups.Dispositions();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AddNote(int id) => View(await _admissions.GetNoteFormAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNote(int id, AdmissionNoteViewModel model)
    {
        model.AdmissionId = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            await _admissions.AddNoteAsync(model);
            Success("Note added.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            AddDomainError(ex, model);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> RecordVitals(int id) => View(await _admissions.GetVitalsFormAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordVitals(int id, VitalsViewModel model)
    {
        model.AdmissionId = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            await _admissions.RecordVitalsAsync(model);
            Success("Vital signs recorded.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            AddDomainError(ex, model);
            return View(model);
        }
    }

    private async Task PopulateAsync(AdmitPatientViewModel model)
    {
        model.Patients = await _lookups.PatientsAsync();
        model.Beds = await _lookups.AvailableBedsAsync();
        model.Physicians = await _lookups.PractitionersAsync(PractitionerType.Physician);
    }
}
