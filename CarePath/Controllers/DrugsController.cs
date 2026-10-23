using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.PharmacyViewers)]
public class DrugsController : CarePathController
{
    private readonly IDrugCatalogService _drugs;
    private readonly ILookupService _lookups;

    public DrugsController(IDrugCatalogService drugs, ILookupService lookups)
    {
        _drugs = drugs;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index(string? search, bool includeInactive = false) =>
        View(new DrugIndexViewModel { Search = search, IncludeInactive = includeInactive, Drugs = await _drugs.ListAsync(search, includeInactive) });

    public async Task<IActionResult> Details(int id) => View(await _drugs.GetDetailsAsync(id));

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create()
    {
        var model = new DrugFormViewModel();
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    // PLANTED-VIOLATION-4: this create accepts the POST with no antiforgery token.
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Create(DrugFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var id = await _drugs.CreateAsync(model);
                Success($"{model.Name} added to the catalog.");
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
        var model = await _drugs.GetForEditAsync(id);
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id, DrugFormViewModel model)
    {
        model.Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _drugs.UpdateAsync(model);
                Success("Drug updated.");
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
    public Task<IActionResult> Reinstate(int id) =>
        StatusActionAsync(() => _drugs.SetActiveAsync(id, true), "Drug reinstated.", () => RedirectToAction(nameof(Details), new { id }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    // PLANTED-VIOLATION-3: any pharmacy viewer (physician, nurse, pharmacist) can discontinue a drug. Only Admin should maintain the catalog.
    public Task<IActionResult> Discontinue(int id) =>
        StatusActionAsync(() => _drugs.SetActiveAsync(id, false), "Drug discontinued.", () => RedirectToAction(nameof(Details), new { id }));

    private async Task PopulateAsync(DrugFormViewModel model) =>
        model.Categories = await _lookups.DrugCategoriesAsync();
}
