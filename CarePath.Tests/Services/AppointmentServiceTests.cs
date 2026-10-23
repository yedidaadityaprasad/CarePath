using CarePath.Exceptions;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class AppointmentServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private AppointmentFormViewModel Form(Patient patient, Practitioner practitioner, DateTime start, int minutes = 30) => new()
    {
        PatientId = patient.Id,
        PractitionerId = practitioner.Id,
        StartsAt = start,
        DurationMinutes = minutes,
        Reason = "Post-discharge review",
        Location = "OPD 3"
    };

    [Fact]
    public async Task Book_stores_end_time_from_duration()
    {
        var start = _ctx.Clock.Now.AddDays(1);

        var id = await _ctx.Appointments.BookAsync(Form(_ctx.Data.Patient(), _ctx.Data.Practitioner(), start, 45));

        var appointment = await _ctx.Fresh().Appointments.SingleAsync(a => a.Id == id);
        Assert.Equal(AppointmentStatus.Booked, appointment.Status);
        Assert.Equal(start.AddMinutes(45), appointment.EndsAt);
    }

    [Fact]
    public async Task Double_booking_the_practitioner_is_rejected()
    {
        var doctor = _ctx.Data.Practitioner();
        var start = _ctx.Clock.Now.AddDays(1);
        await _ctx.Appointments.BookAsync(Form(_ctx.Data.Patient(), doctor, start));

        await Assert.ThrowsAsync<SchedulingConflictException>(() =>
            _ctx.Appointments.BookAsync(Form(_ctx.Data.Patient(), doctor, start.AddMinutes(15))));
    }

    [Fact]
    public async Task Booking_in_the_past_is_rejected()
    {
        var ex = await Assert.ThrowsAsync<DomainValidationException>(() =>
            _ctx.Appointments.BookAsync(Form(_ctx.Data.Patient(), _ctx.Data.Practitioner(), _ctx.Clock.Now.AddHours(-1))));
        Assert.Equal(nameof(AppointmentFormViewModel.StartsAt), ex.PropertyName);
    }

    [Fact]
    public async Task Complete_after_start_records_notes_and_history()
    {
        var id = await _ctx.Appointments.BookAsync(Form(_ctx.Data.Patient(), _ctx.Data.Practitioner(), _ctx.Clock.Now.AddHours(2)));
        _ctx.Clock.Advance(TimeSpan.FromHours(2.5));

        await _ctx.Appointments.CompleteAsync(id, "Wound healing well");

        using var db = _ctx.Fresh();
        var appointment = await db.Appointments.Include(a => a.StatusChanges).SingleAsync(a => a.Id == id);
        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
        Assert.Equal("Wound healing well", appointment.CompletionNotes);
        Assert.Single(appointment.StatusChanges, c => c.ToStatus == AppointmentStatus.Completed);
    }

    [Fact]
    public async Task Complete_before_start_is_rejected()
    {
        var id = await _ctx.Appointments.BookAsync(Form(_ctx.Data.Patient(), _ctx.Data.Practitioner(), _ctx.Clock.Now.AddDays(1)));

        await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Appointments.CompleteAsync(id, null));
    }

    [Fact]
    public async Task Cancelled_appointment_cannot_be_completed()
    {
        var id = await _ctx.Appointments.BookAsync(Form(_ctx.Data.Patient(), _ctx.Data.Practitioner(), _ctx.Clock.Now.AddDays(1)));
        await _ctx.Appointments.CancelAsync(id, "Patient request");

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() => _ctx.Appointments.CompleteAsync(id, null));
    }
}
