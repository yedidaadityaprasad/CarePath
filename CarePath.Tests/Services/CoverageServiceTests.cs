using CarePath.Exceptions;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class CoverageServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private CoverageFormViewModel Form(int patientId, int payerId, DateTime from, DateTime? to = null, decimal? percent = null) => new()
    {
        PatientId = patientId,
        PayerId = payerId,
        MemberId = "MEM-001",
        EffectiveFrom = from,
        EffectiveTo = to,
        CoveragePercent = percent
    };

    [Fact]
    public async Task Attach_uses_payer_default_percent_when_blank()
    {
        var patient = _ctx.Data.Patient();
        var payer = _ctx.Data.Payer(defaultPercent: 70m);

        var id = await _ctx.Coverage.AttachAsync(Form(patient.Id, payer.Id, _ctx.Clock.Now.Date));

        var saved = await _ctx.Fresh().InsuranceCoverages.SingleAsync(c => c.Id == id);
        Assert.Equal(70m, saved.CoveragePercent);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task Attach_rejects_overlapping_period_for_same_payer()
    {
        var patient = _ctx.Data.Patient();
        var payer = _ctx.Data.Payer();
        await _ctx.Coverage.AttachAsync(Form(patient.Id, payer.Id, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)));

        await Assert.ThrowsAsync<CoverageConflictException>(() =>
            _ctx.Coverage.AttachAsync(Form(patient.Id, payer.Id, new DateTime(2026, 6, 1))));
    }

    [Fact]
    public async Task Attach_rejects_inactive_patient()
    {
        var patient = _ctx.Data.Patient(active: false);
        var payer = _ctx.Data.Payer();

        await Assert.ThrowsAsync<PatientInactiveException>(() => _ctx.Coverage.AttachAsync(Form(patient.Id, payer.Id, _ctx.Clock.Now.Date)));
    }

    [Fact]
    public async Task FindActiveCoverage_prefers_highest_percentage_on_date()
    {
        var patient = _ctx.Data.Patient();
        _ctx.Data.Coverage(patient, _ctx.Data.Payer(), 60m);
        var best = _ctx.Data.Coverage(patient, _ctx.Data.Payer(), 90m);
        _ctx.Data.Coverage(patient, _ctx.Data.Payer(), 100m, from: _ctx.Clock.Now.Date.AddDays(10));

        var found = await _ctx.Coverage.FindActiveCoverageAsync(patient.Id, _ctx.Clock.Now);

        Assert.NotNull(found);
        Assert.Equal(best.Id, found!.Id);
    }

    [Fact]
    public async Task Terminate_is_blocked_while_an_open_admission_uses_the_coverage()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);

        await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Coverage.TerminateAsync(admitted.Coverage.Id, "Policy lapsed"));
    }

    [Fact]
    public async Task Terminate_closes_open_ended_coverage_today()
    {
        var patient = _ctx.Data.Patient();
        var coverage = _ctx.Data.Coverage(patient, _ctx.Data.Payer());

        await _ctx.Coverage.TerminateAsync(coverage.Id, "Policy lapsed");

        var saved = await _ctx.Fresh().InsuranceCoverages.SingleAsync(c => c.Id == coverage.Id);
        Assert.False(saved.IsActive);
        Assert.Equal(_ctx.Clock.Now.Date, saved.EffectiveTo);
    }
}
