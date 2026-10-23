using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.Clinical)]
public class AppointmentsController : CarePathController
{
    private readonly IAppointmentService _appointments;
    private readonly ILookupService _lookups;

    public AppointmentsController(IAppointmentService appointments, ILookupService lookups)
    {
        _appointments = appointments;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index(DateTime? from, DateTime? to, int? practitionerId, AppointmentStatus? status)
    {
        var model = new AppointmentIndexViewModel
        {
            From = from ?? DateTime.Today,
            To = to ?? DateTime.Today.AddDays(30),
            PractitionerId = practitionerId,
            Status = status,
            Practitioners = await _lookups.PractitionersAsync(activeOnly: false)
        };
        try
        {
            model.Appointments = await _appointments.SearchAsync(model.From, model.To, practitionerId, status);
        }
        catch (DomainException ex)
        {
            AddDomainError(ex, model);
        }
        return View(model);
    }

    public async Task<IActionResult> Details(int id) => View(await _appointments.GetDetailsAsync(id));

    [HttpGet]
    public async Task<IActionResult> Create(int? patientId, int? practitionerId)
    {
        var model = new AppointmentFormViewModel
        {
            PatientId = patientId,
            PractitionerId = practitionerId,
            StartsAt = DateTime.Today.AddDays(1).AddHours(9)
        };
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var id = await _appointments.BookAsync(model);
                Success("Appointment booked.");
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
        var model = await _appointments.GetForEditAsync(id);
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppointmentFormViewModel model)
    {
        model.Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _appointments.RescheduleAsync(model);
                Success("Appointment rescheduled.");
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
        ReasonActionAsync(input, reason => _appointments.CancelAsync(input.Id, reason),
            "Appointment cancelled.", () => RedirectToAction(nameof(Details), new { id = input.Id }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Complete(CompleteAppointmentViewModel input)
    {
        if (!ModelState.IsValid)
        {
            Failure("Completion notes must be 2000 characters or fewer.");
            return Task.FromResult<IActionResult>(RedirectToAction(nameof(Details), new { id = input.Id }));
        }
        return StatusActionAsync(() => _appointments.CompleteAsync(input.Id, input.Notes),
            "Appointment completed.", () => RedirectToAction(nameof(Details), new { id = input.Id }));
    }

    private async Task PopulateAsync(AppointmentFormViewModel model)
    {
        model.Patients = await _lookups.PatientsAsync();
        model.Practitioners = await _lookups.PractitionersAsync();
    }
}
