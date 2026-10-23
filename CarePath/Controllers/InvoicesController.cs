using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.BillingViewers)]
public class InvoicesController : CarePathController
{
    private readonly IBillingService _billing;

    public InvoicesController(IBillingService billing)
    {
        _billing = billing;
    }

    public async Task<IActionResult> Index(InvoiceStatus? status, string? search, int page = 1) =>
        View(new InvoiceIndexViewModel
        {
            Status = status,
            Search = search,
            Page = page,
            Results = await _billing.SearchAsync(status, search, page)
        });

    public async Task<IActionResult> Details(int id) => View(await _billing.GetDetailsAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.BillingClerk)]
    public Task<IActionResult> Issue(IssueInvoiceViewModel input)
    {
        if (!ModelState.IsValid || input.DueInDays is null)
        {
            Failure("Payment terms must be between 1 and 180 days.");
            return Task.FromResult<IActionResult>(RedirectToAction(nameof(Details), new { id = input.InvoiceId }));
        }
        return StatusActionAsync(() => _billing.IssueAsync(input.InvoiceId, input.DueInDays.Value),
            "Invoice issued.", () => RedirectToAction(nameof(Details), new { id = input.InvoiceId }));
    }

    [HttpGet]
    [Authorize(Roles = Roles.BillingClerk)]
    public async Task<IActionResult> Payment(int id)
    {
        try
        {
            return View(await _billing.GetPaymentFormAsync(id));
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            Failure(ex.Message);
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.BillingClerk)]
    public async Task<IActionResult> Payment(int id, PaymentViewModel model)
    {
        model.InvoiceId = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _billing.RecordPaymentAsync(model);
                Success($"Payment of {model.Amount:0.00} recorded.");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DomainException ex) when (ex is not EntityNotFoundException)
            {
                AddDomainError(ex, model);
            }
        }
        var fresh = await _billing.GetPaymentFormAsync(id);
        model.InvoiceNumber = fresh.InvoiceNumber;
        model.PatientName = fresh.PatientName;
        model.BalanceDue = fresh.BalanceDue;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.BillingClerk)]
    public Task<IActionResult> Void(ReasonActionViewModel input) =>
        ReasonActionAsync(input, reason => _billing.VoidAsync(input.Id, reason),
            "Invoice voided.", () => RedirectToAction(nameof(Details), new { id = input.Id }));
}
