using CarePath.Exceptions;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class ShiftServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private ShiftFormViewModel Form(Practitioner practitioner, Ward ward, DateTime start, double hours) => new()
    {
        PractitionerId = practitioner.Id,
        WardId = ward.Id,
        StartsAt = start,
        EndsAt = start.AddHours(hours)
    };

    [Fact]
    public async Task Schedule_saves_shift_and_audits()
    {
        var nurse = _ctx.Data.Practitioner(PractitionerType.Nurse);
        var ward = _ctx.Data.Ward();

        var id = await _ctx.Shifts.ScheduleAsync(Form(nurse, ward, _ctx.Clock.Now.Date.AddDays(1).AddHours(7), 12));

        using var db = _ctx.Fresh();
        var shift = await db.Shifts.SingleAsync(s => s.Id == id);
        Assert.Equal(ShiftStatus.Scheduled, shift.Status);
        Assert.Equal(_ctx.User.UserName, shift.CreatedBy);
        Assert.True(await db.AuditEntries.AnyAsync(e => e.EntityName == nameof(Shift) && e.EntityId == id.ToString()));
    }

    [Fact]
    public async Task Overlapping_shift_for_same_practitioner_is_rejected()
    {
        var nurse = _ctx.Data.Practitioner(PractitionerType.Nurse);
        var start = _ctx.Clock.Now.Date.AddDays(1).AddHours(7);
        await _ctx.Shifts.ScheduleAsync(Form(nurse, _ctx.Data.Ward(), start, 12));

        await Assert.ThrowsAsync<SchedulingConflictException>(() =>
            _ctx.Shifts.ScheduleAsync(Form(nurse, _ctx.Data.Ward(), start.AddHours(11), 8)));
    }

    [Fact]
    public async Task Shift_longer_than_sixteen_hours_is_rejected()
    {
        var nurse = _ctx.Data.Practitioner(PractitionerType.Nurse);

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() =>
            _ctx.Shifts.ScheduleAsync(Form(nurse, _ctx.Data.Ward(), _ctx.Clock.Now.AddDays(1), 17)));
        Assert.Equal(nameof(ShiftFormViewModel.EndsAt), ex.PropertyName);
    }

    [Fact]
    public async Task Shift_on_closed_ward_is_rejected()
    {
        var nurse = _ctx.Data.Practitioner(PractitionerType.Nurse);

        await Assert.ThrowsAsync<WardClosedException>(() =>
            _ctx.Shifts.ScheduleAsync(Form(nurse, _ctx.Data.Ward(closed: true), _ctx.Clock.Now.AddDays(1), 8)));
    }

    [Fact]
    public async Task Cancelled_shift_cannot_be_cancelled_again()
    {
        var nurse = _ctx.Data.Practitioner(PractitionerType.Nurse);
        var id = await _ctx.Shifts.ScheduleAsync(Form(nurse, _ctx.Data.Ward(), _ctx.Clock.Now.AddDays(2), 8));
        await _ctx.Shifts.CancelAsync(id, "Sick leave");

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() => _ctx.Shifts.CancelAsync(id, "Again"));
    }
}
