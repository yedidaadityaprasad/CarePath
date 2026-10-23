using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.PharmacyViewers)]
public class PharmacyController : CarePathController
{
    private readonly IPharmacyService _pharmacy;
    private readonly ILookupService _lookups;

    public PharmacyController(IPharmacyService pharmacy, ILookupService lookups)
    {
        _pharmacy = pharmacy;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index(int? wardId, bool lowStockOnly = false) =>
        View(new WardStockIndexViewModel
        {
            WardId = wardId,
            LowStockOnly = lowStockOnly,
            Stock = await _pharmacy.ListStockAsync(wardId, lowStockOnly),
            Wards = await _lookups.WardsAsync()
        });

    public async Task<IActionResult> Details(int id) => View(await _pharmacy.GetStockDetailsAsync(id));

    [HttpGet]
    [Authorize(Roles = Roles.PharmacyAndAdmin)]
    public async Task<IActionResult> Create(int? wardId)
    {
        var model = new WardStockFormViewModel { WardId = wardId };
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.PharmacyAndAdmin)]
    public async Task<IActionResult> Create(WardStockFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var id = await _pharmacy.CreateStockAsync(model);
                Success("Stock line created.");
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
    [Authorize(Roles = Roles.PharmacyAndAdmin)]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _pharmacy.GetStockForEditAsync(id);
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.PharmacyAndAdmin)]
    public async Task<IActionResult> Edit(int id, WardStockFormViewModel model)
    {
        model.Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _pharmacy.UpdateStockAsync(model);
                Success("Reorder level updated.");
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

    [HttpGet]
    [Authorize(Roles = Roles.Pharmacist)]
    public async Task<IActionResult> Restock(int id) => View(await _pharmacy.GetRestockFormAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Pharmacist)]
    public async Task<IActionResult> Restock(int id, RestockViewModel model)
    {
        model.StockId = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _pharmacy.RestockAsync(model);
                Success($"Restocked {model.Quantity} unit(s).");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DomainException ex) when (ex is not EntityNotFoundException)
            {
                AddDomainError(ex, model);
            }
        }
        var fresh = await _pharmacy.GetRestockFormAsync(id);
        model.WardName = fresh.WardName;
        model.DrugName = fresh.DrugName;
        model.QuantityOnHand = fresh.QuantityOnHand;
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Pharmacist)]
    public async Task<IActionResult> Queue() => View(await _pharmacy.GetDispenseQueueAsync());

    [HttpGet]
    [Authorize(Roles = Roles.Pharmacist)]
    public async Task<IActionResult> Dispense(int id)
    {
        try
        {
            return View(await _pharmacy.GetDispenseFormAsync(id));
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            Failure(ex.Message);
            return RedirectToAction(nameof(Queue));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Pharmacist)]
    public async Task<IActionResult> Dispense(int id, DispenseViewModel model)
    {
        model.OrderId = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _pharmacy.DispenseAsync(id, model.Notes);
                Success("Medication dispensed and order completed.");
                return RedirectToAction(nameof(Queue));
            }
            catch (DomainException ex) when (ex is not EntityNotFoundException)
            {
                AddDomainError(ex, model);
            }
        }
        var fresh = await _pharmacy.GetDispenseFormAsync(id);
        fresh.Notes = model.Notes;
        return View(fresh);
    }

    private async Task PopulateAsync(WardStockFormViewModel model)
    {
        model.Wards = await _lookups.WardsAsync();
        model.Drugs = await _lookups.DrugsAsync(activeOnly: model.Id == 0);
    }
}
