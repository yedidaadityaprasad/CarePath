using CarePath.Exceptions;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class PractitionerServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public async Task Create_normalises_license_and_saves()
    {
        var id = await _ctx.Practitioners.CreateAsync(new PractitionerFormViewModel
        {
            FirstName = "Kavya",
            LastName = "Rao",
            Type = PractitionerType.Physician,
            LicenseNumber = "mci-4411",
            SpecialtyId = 2
        });

        var saved = await _ctx.Fresh().Practitioners.SingleAsync(p => p.Id == id);
        Assert.Equal("MCI-4411", saved.LicenseNumber);
        Assert.Equal("Dr. Kavya Rao", saved.DisplayName);
    }

    [Fact]
    public async Task Create_rejects_duplicate_license()
    {
        var existing = _ctx.Data.Practitioner();

        await Assert.ThrowsAsync<DuplicateValueException>(() => _ctx.Practitioners.CreateAsync(new PractitionerFormViewModel
        {
            FirstName = "Other",
            LastName = "Person",
            Type = PractitionerType.Nurse,
            LicenseNumber = existing.LicenseNumber.ToLowerInvariant()
        }));
    }

    [Fact]
    public async Task Deactivate_cancels_future_shifts_and_records_history()
    {
        var nurse = _ctx.Data.Practitioner(PractitionerType.Nurse);
        var ward = _ctx.Data.Ward();
        var shiftId = await _ctx.Shifts.ScheduleAsync(new ShiftFormViewModel
        {
            PractitionerId = nurse.Id,
            WardId = ward.Id,
            StartsAt = _ctx.Clock.Now.AddDays(1),
            EndsAt = _ctx.Clock.Now.AddDays(1).AddHours(8)
        });

        await _ctx.Practitioners.DeactivateAsync(nurse.Id, "Resigned");

        using var db = _ctx.Fresh();
        Assert.False((await db.Practitioners.SingleAsync(p => p.Id == nurse.Id)).IsActive);
        Assert.Equal(ShiftStatus.Cancelled, (await db.Shifts.SingleAsync(s => s.Id == shiftId)).Status);
        Assert.Single(await db.PractitionerStatusChanges.Where(c => c.PractitionerId == nurse.Id && !c.IsActive).ToListAsync());
    }

    [Fact]
    public async Task Deactivate_is_rejected_while_attending_open_admissions()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);

        await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Practitioners.DeactivateAsync(admitted.Physician.Id, "Sabbatical"));
    }
}
