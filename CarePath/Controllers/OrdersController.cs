using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.Clinical)]
public class OrdersController : CarePathController
{
    private readonly IOrderService _orders;
    private readonly ILookupService _lookups;

    public OrdersController(IOrderService orders, ILookupService lookups)
    {
        _orders = orders;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index(OrderType? type, OrderStatus? status, string? search, int page = 1) =>
        View(new OrderIndexViewModel
        {
            Type = type,
            Status = status,
            Search = search,
            Page = page,
            Results = await _orders.SearchAsync(type, status, search, page)
        });

    public async Task<IActionResult> Details(int id) => View(await _orders.GetDetailsAsync(id));

    [HttpGet]
    public async Task<IActionResult> CreateLab(int? admissionId)
    {
        var model = new LabOrderFormViewModel { AdmissionId = admissionId };
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLab(LabOrderFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var id = await _orders.PlaceLabOrderAsync(model);
                Success("Lab order placed.");
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
    public async Task<IActionResult> CreateMedication(int? admissionId)
    {
        var model = new MedicationOrderFormViewModel { AdmissionId = admissionId };
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMedication(MedicationOrderFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var id = await _orders.PlaceMedicationOrderAsync(model);
                Success("Medication order placed.");
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
    public async Task<IActionResult> Edit(int id) => View(await _orders.GetForEditAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OrderEditViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            await _orders.UpdateAsync(model);
            Success("Order updated.");
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
    public Task<IActionResult> Cancel(ReasonActionViewModel input) =>
        ReasonActionAsync(input, reason => _orders.CancelAsync(input.Id, reason),
            "Order cancelled.", () => RedirectToAction(nameof(Details), new { id = input.Id }));

    private async Task PopulateAsync(LabOrderFormViewModel model)
    {
        model.Admissions = await _lookups.OpenAdmissionsAsync();
        model.Practitioners = await _lookups.PractitionersAsync();
        model.Tests = await _lookups.LabTestsAsync();
    }

    private async Task PopulateAsync(MedicationOrderFormViewModel model)
    {
        model.Admissions = await _lookups.OpenAdmissionsAsync();
        model.Practitioners = await _lookups.PractitionersAsync(PractitionerType.Physician);
        model.Drugs = await _lookups.DrugsAsync();
    }
}
