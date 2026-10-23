using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.Clinical)]
public class LabController : CarePathController
{
    private readonly ILabService _lab;

    public LabController(ILabService lab)
    {
        _lab = lab;
    }

    public async Task<IActionResult> Index(OrderStatus? status) =>
        View(new LabWorklistViewModel { Status = status, Orders = await _lab.GetWorklistAsync(status) });

    public async Task<IActionResult> Details(int id) => View(await _lab.GetOrderAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Accept(int id) =>
        StatusActionAsync(() => _lab.AcceptAsync(id), "Lab order accepted.", () => RedirectToAction(nameof(Details), new { id }));

    [HttpGet]
    public async Task<IActionResult> Specimen(int id)
    {
        try
        {
            return View(await _lab.GetSpecimenFormAsync(id));
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            Failure(ex.Message);
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Specimen(int id, SpecimenFormViewModel model)
    {
        model.OrderId = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            await _lab.RecordSpecimenAsync(model);
            Success("Specimen recorded.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            AddDomainError(ex, model);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Result(int id)
    {
        try
        {
            return View(await _lab.GetResultFormAsync(id));
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            Failure(ex.Message);
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Result(int id, LabResultFormViewModel model)
    {
        model.OrderId = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            await _lab.EnterResultAsync(model);
            Success("Result entered and order completed.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            AddDomainError(ex, model);
            return View(model);
        }
    }
}
