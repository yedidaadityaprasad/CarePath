using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.Clinical)]
public class PatientsController : CarePathController
{
    private readonly IPatientService _patients;

    public PatientsController(IPatientService patients)
    {
        _patients = patients;
    }

    public async Task<IActionResult> Index(string? search, bool includeInactive = false, int page = 1)
    {
        var model = new PatientIndexViewModel
        {
            Search = search,
            IncludeInactive = includeInactive,
            Page = page,
            Results = await _patients.SearchAsync(search, includeInactive, page)
        };
        return View(model);
    }

    public async Task<IActionResult> Details(int id) => View(await _patients.GetDetailsAsync(id));

    [HttpGet]
    public IActionResult Create() => View(new PatientFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PatientFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            var id = await _patients.RegisterAsync(model);
            Success($"Patient {model.MedicalRecordNumber!.ToUpperInvariant()} registered.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DomainException ex)
        {
            AddDomainError(ex, model);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id) => View(await _patients.GetForEditAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PatientFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            await _patients.UpdateAsync(model);
            Success("Demographics updated.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            AddDomainError(ex, model);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Deactivate(ReasonActionViewModel input) =>
        ReasonActionAsync(input, reason => _patients.DeactivateAsync(input.Id, reason),
            "Patient deactivated.", () => RedirectToAction(nameof(Details), new { id = input.Id }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Reactivate(int id) =>
        StatusActionAsync(() => _patients.ReactivateAsync(id), "Patient reactivated.", () => RedirectToAction(nameof(Details), new { id }));

    [HttpGet]
    public async Task<IActionResult> AddAllergy(int id) => View(await _patients.GetAllergyFormAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAllergy(AllergyFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            await _patients.AddAllergyAsync(model);
            Success($"Allergy to {model.Substance} recorded.");
            return RedirectToAction(nameof(Details), new { id = model.PatientId });
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            AddDomainError(ex, model);
            return View(model);
        }
    }
}
