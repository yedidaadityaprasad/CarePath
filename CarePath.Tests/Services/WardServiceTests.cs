using CarePath.Exceptions;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class WardServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public async Task Create_ward_and_bed_then_summary_counts_beds()
    {
        var wardId = await _ctx.Wards.CreateWardAsync(new WardFormViewModel { Code = "icu-1", Name = "ICU", DepartmentId = 3, Floor = 4, DailyRate = 900m });
        await _ctx.Wards.AddBedAsync(new BedFormViewModel { WardId = wardId, BedNumber = "a1" });
        await _ctx.Wards.AddBedAsync(new BedFormViewModel { WardId = wardId, BedNumber = "a2" });

        var summary = Assert.Single(await _ctx.Wards.ListWardsAsync(includeClosed: true));
        Assert.Equal("ICU-1", summary.Ward.Code);
        Assert.Equal(2, summary.TotalBeds);
        Assert.Equal(2, summary.Available);
    }

    [Fact]
    public async Task AddBed_rejects_duplicate_number_in_same_ward()
    {
        var ward = _ctx.Data.Ward();
        _ctx.Data.Bed(ward, "A1");

        await Assert.ThrowsAsync<DuplicateValueException>(() => _ctx.Wards.AddBedAsync(new BedFormViewModel { WardId = ward.Id, BedNumber = "a1" }));
    }

    [Fact]
    public async Task Close_ward_is_rejected_while_a_bed_is_occupied()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);

        await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Wards.CloseWardAsync(admitted.Ward.Id, "Refurbishment"));
        Assert.False((await _ctx.Fresh().Wards.SingleAsync(w => w.Id == admitted.Ward.Id)).IsClosed);
    }

    [Fact]
    public async Task Out_of_service_and_back_records_status_history()
    {
        var bed = _ctx.Data.Bed(_ctx.Data.Ward());

        await _ctx.Wards.SetBedOutOfServiceAsync(bed.Id, "Broken rail");
        await _ctx.Wards.ReturnBedToServiceAsync(bed.Id, "Repaired");

        using var db = _ctx.Fresh();
        Assert.Equal(BedStatus.Available, (await db.Beds.SingleAsync(b => b.Id == bed.Id)).Status);
        Assert.Equal(2, await db.BedStatusChanges.CountAsync(c => c.BedId == bed.Id));
    }

    [Fact]
    public async Task Occupied_bed_cannot_be_taken_out_of_service()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() => _ctx.Wards.SetBedOutOfServiceAsync(admitted.Bed.Id, "Cleaning"));
    }
}
