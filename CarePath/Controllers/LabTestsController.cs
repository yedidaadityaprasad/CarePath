using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.ClinicalAndAdmin)]
public class LabTestsController : CarePathController
{
    private readonly ILabTestCatalogService _tests;
    private readonly ILookupService _lookups;

    public LabTestsController(ILabTestCatalogService tests, ILookupService lookups)
    {
        _tests = tests;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index(bool includeInactive = false) =>
        View(new LabTestIndexViewModel { IncludeInactive = includeInactive, Tests = await _tests.ListAsync(includeInactive) });

    public async Task<IActionResult> Details(int id) => View(await _tests.GetDetailsAsync(id));

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create()
    {
        var model = new LabTestFormViewModel();
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(LabTestFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var id = await _tests.CreateAsync(model);
                Success($"Lab test {model.Name} created.");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DomainException ex)
            {
                AddDomainError(ex, model);
            }
        }
        await PopulateAsync(model);
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _tests.GetForEditAsync(id);
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id, LabTestFormViewModel model)
    {
        model.Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _tests.UpdateAsync(model);
                Success("Lab test updated.");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DomainException ex) when (ex is not EntityNotFoundException)
            {
                AddDomainError(ex, model);
            }
        }
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public Task<IActionResult> Activate(int id) =>
        StatusActionAsync(() => _tests.SetActiveAsync(id, true), "Lab test reactivated.", () => RedirectToAction(nameof(Details), new { id }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public Task<IActionResult> Retire(int id) =>
        StatusActionAsync(() => _tests.SetActiveAsync(id, false), "Lab test retired.", () => RedirectToAction(nameof(Details), new { id }));

    private async Task PopulateAsync(LabTestFormViewModel model) =>
        model.SpecimenTypes = await _lookups.SpecimenTypesAsync();
}
