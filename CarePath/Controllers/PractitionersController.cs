using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.ClinicalAndAdmin)]
public class PractitionersController : CarePathController
{
    private readonly IPractitionerService _practitioners;
    private readonly ILookupService _lookups;

    public PractitionersController(IPractitionerService practitioners, ILookupService lookups)
    {
        _practitioners = practitioners;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index(string? search, PractitionerType? type, bool activeOnly = false) =>
        View(new PractitionerIndexViewModel
        {
            Search = search,
            Type = type,
            ActiveOnly = activeOnly,
            Practitioners = await _practitioners.SearchAsync(search, type, activeOnly)
        });

    public async Task<IActionResult> Details(int id) => View(await _practitioners.GetDetailsAsync(id));

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create()
    {
        var model = new PractitionerFormViewModel();
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(PractitionerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model);
            return View(model);
        }
        try
        {
            var id = await _practitioners.CreateAsync(model);
            Success("Practitioner created.");
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
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _practitioners.GetForEditAsync(id);
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id, PractitionerFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model);
            return View(model);
        }
        try
        {
            await _practitioners.UpdateAsync(model);
            Success("Practitioner updated.");
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
    [Authorize(Roles = Roles.Admin)]
    public Task<IActionResult> Deactivate(ReasonActionViewModel input) =>
        ReasonActionAsync(input, reason => _practitioners.DeactivateAsync(input.Id, reason),
            "Practitioner deactivated.", () => RedirectToAction(nameof(Details), new { id = input.Id }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public Task<IActionResult> Activate(ReasonActionViewModel input) =>
        ReasonActionAsync(input, reason => _practitioners.ActivateAsync(input.Id, reason),
            "Practitioner activated.", () => RedirectToAction(nameof(Details), new { id = input.Id }));

    private async Task PopulateAsync(PractitionerFormViewModel model) =>
        model.Specialties = await _lookups.SpecialtiesAsync();
}
