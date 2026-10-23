using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.Clinical)]
public class ShiftsController : CarePathController
{
    private readonly IShiftService _shifts;
    private readonly ILookupService _lookups;

    public ShiftsController(IShiftService shifts, ILookupService lookups)
    {
        _shifts = shifts;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index(int? wardId, int? practitionerId, DateTime? from, DateTime? to, bool includeCancelled = false)
    {
        var model = new ShiftIndexViewModel
        {
            WardId = wardId,
            PractitionerId = practitionerId,
            From = from ?? DateTime.Today,
            To = to ?? DateTime.Today.AddDays(14),
            IncludeCancelled = includeCancelled,
            Wards = await _lookups.WardsAsync(),
            Practitioners = await _lookups.PractitionersAsync(activeOnly: false)
        };
        try
        {
            model.Shifts = await _shifts.SearchAsync(wardId, practitionerId, model.From, model.To, includeCancelled);
        }
        catch (DomainException ex)
        {
            AddDomainError(ex, model);
        }
        return View(model);
    }

    public async Task<IActionResult> Details(int id) => View(await _shifts.GetDetailsAsync(id));

    [HttpGet]
    public async Task<IActionResult> Create(int? wardId)
    {
        var start = DateTime.Today.AddDays(1).AddHours(7);
        var model = new ShiftFormViewModel { WardId = wardId, StartsAt = start, EndsAt = start.AddHours(12) };
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShiftFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var id = await _shifts.ScheduleAsync(model);
                Success("Shift scheduled.");
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
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _shifts.GetForEditAsync(id);
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ShiftFormViewModel model)
    {
        model.Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _shifts.UpdateAsync(model);
                Success("Shift updated.");
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
    public Task<IActionResult> Cancel(ReasonActionViewModel input) =>
        ReasonActionAsync(input, reason => _shifts.CancelAsync(input.Id, reason),
            "Shift cancelled.", () => RedirectToAction(nameof(Details), new { id = input.Id }));

    private async Task PopulateAsync(ShiftFormViewModel model)
    {
        model.Practitioners = await _lookups.PractitionersAsync();
        model.Wards = await _lookups.WardsAsync(openOnly: true);
    }
}
