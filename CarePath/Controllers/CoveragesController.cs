using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.CoverageViewers)]
public class CoveragesController : CarePathController
{
    private readonly ICoverageService _coverage;
    private readonly ILookupService _lookups;

    public CoveragesController(ICoverageService coverage, ILookupService lookups)
    {
        _coverage = coverage;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index(string? search, bool activeOnly = true, int page = 1)
    {
        return View(new CoverageIndexViewModel
        {
            Search = search,
            ActiveOnly = activeOnly,
            Page = page,
            Results = await _coverage.SearchAsync(search, activeOnly, page)
        });
    }

    public async Task<IActionResult> Details(int id) => View(await _coverage.GetDetailsAsync(id));

    [HttpGet]
    [Authorize(Roles = Roles.CoverageEditors)]
    public async Task<IActionResult> Create(int? patientId)
    {
        var model = new CoverageFormViewModel { PatientId = patientId, EffectiveFrom = DateTime.Today };
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.CoverageEditors)]
    public async Task<IActionResult> Create(CoverageFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model);
            return View(model);
        }
        try
        {
            var id = await _coverage.AttachAsync(model);
            Success("Coverage attached.");
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
    [Authorize(Roles = Roles.CoverageEditors)]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _coverage.GetForEditAsync(id);
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.CoverageEditors)]
    public async Task<IActionResult> Edit(int id, CoverageFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model);
            return View(model);
        }
        try
        {
            await _coverage.UpdateAsync(model);
            Success("Coverage updated.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            AddDomainError(ex, model);
            await PopulateAsync(model);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.CoverageEditors)]
    public Task<IActionResult> Terminate(ReasonActionViewModel input) =>
        ReasonActionAsync(input, reason => _coverage.TerminateAsync(input.Id, reason),
            "Coverage terminated.", () => RedirectToAction(nameof(Details), new { id = input.Id }));

    private async Task PopulateAsync(CoverageFormViewModel model)
    {
        model.Patients = await _lookups.PatientsAsync();
        model.Payers = await _lookups.PayersAsync();
    }
}
