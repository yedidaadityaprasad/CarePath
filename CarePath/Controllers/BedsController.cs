using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.ClinicalAndAdmin)]
public class BedsController : CarePathController
{
    private readonly IWardService _wards;
    private readonly ILookupService _lookups;

    public BedsController(IWardService wards, ILookupService lookups)
    {
        _wards = wards;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index(int? wardId, BedStatus? status) =>
        View(new BedIndexViewModel
        {
            WardId = wardId,
            Status = status,
            Beds = await _wards.ListBedsAsync(wardId, status),
            Wards = await _lookups.WardsAsync()
        });

    public async Task<IActionResult> Details(int id) => View(await _wards.GetBedDetailsAsync(id));

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(int? wardId)
    {
        var model = new BedFormViewModel { WardId = wardId };
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(BedFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model);
            return View(model);
        }
        try
        {
            var id = await _wards.AddBedAsync(model);
            Success($"Bed {model.BedNumber} added.");
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
        var model = await _wards.GetBedForEditAsync(id);
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id, BedFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model);
            return View(model);
        }
        try
        {
            await _wards.UpdateBedAsync(model);
            Success("Bed updated.");
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
    public Task<IActionResult> OutOfService(ReasonActionViewModel input) =>
        ReasonActionAsync(input, reason => _wards.SetBedOutOfServiceAsync(input.Id, reason),
            "Bed taken out of service.", () => RedirectToAction(nameof(Details), new { id = input.Id }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public Task<IActionResult> ReturnToService(ReasonActionViewModel input) =>
        ReasonActionAsync(input, reason => _wards.ReturnBedToServiceAsync(input.Id, reason),
            "Bed returned to service.", () => RedirectToAction(nameof(Details), new { id = input.Id }));

    private async Task PopulateAsync(BedFormViewModel model) =>
        model.Wards = await _lookups.WardsAsync();
}
