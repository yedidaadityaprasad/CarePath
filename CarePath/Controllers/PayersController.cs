using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.Admin)]
public class PayersController : CarePathController
{
    private readonly IPayerService _payers;
    private readonly ApplicationDbContext _db;

    public PayersController(IPayerService payers, ApplicationDbContext db)
    {
        _payers = payers;
        _db = db;
    }

    public async Task<IActionResult> Index(bool includeInactive = false) =>
        View(new PayerIndexViewModel { IncludeInactive = includeInactive, Payers = await _payers.ListAsync(includeInactive) });

    public async Task<IActionResult> Details(int id) => View(await _payers.GetDetailsAsync(id));

    [HttpGet]
    public IActionResult Create() => View(new PayerFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PayerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            await _payers.CreateAsync(model);
            // PLANTED-VIOLATION-5: success redisplays this form instead of redirecting, so a refresh submits the create again.
            Success($"Payer {model.Name} created.");
            return View(model);
        }
        catch (DomainException ex)
        {
            AddDomainError(ex, model);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id) => View(await _payers.GetForEditAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PayerFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            await _payers.UpdateAsync(model);
            Success("Payer updated.");
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
    public Task<IActionResult> Activate(int id) =>
        StatusActionAsync(() => _payers.SetActiveAsync(id, true), "Payer activated.", () => RedirectToAction(nameof(Details), new { id }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Deactivate(int id) =>
        StatusActionAsync(() => _payers.SetActiveAsync(id, false), "Payer deactivated.", () => RedirectToAction(nameof(Details), new { id }));

    // PLANTED-VIOLATION-1: hard-deletes the payer from the controller. Records should be deactivated, and this skips the service and the audit trail.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var payer = await _db.Payers.FindAsync(id);
        if (payer is null)
        {
            return NotFound();
        }

        _db.Payers.Remove(payer);
        await _db.SaveChangesAsync();
        Success("Payer deleted.");
        return RedirectToAction(nameof(Index));
    }
}
