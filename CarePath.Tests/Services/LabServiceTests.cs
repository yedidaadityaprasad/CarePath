using CarePath.Exceptions;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class LabServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private async Task<int> AcceptedOrderWithSpecimenAsync()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var orderId = await _ctx.Orders.PlaceLabOrderAsync(new LabOrderFormViewModel
        {
            AdmissionId = admitted.AdmissionId,
            OrderingPractitionerId = admitted.Physician.Id,
            LabTestDefinitionId = _ctx.Data.NumericLabTest(low: 12m, high: 16m).Id
        });
        await _ctx.Lab.AcceptAsync(orderId);
        await _ctx.Lab.RecordSpecimenAsync(new SpecimenFormViewModel { OrderId = orderId, CollectedAt = _ctx.Clock.Now });
        return orderId;
    }

    [Fact]
    public async Task Result_below_range_is_flagged_low_and_completes_order()
    {
        var orderId = await AcceptedOrderWithSpecimenAsync();

        await _ctx.Lab.EnterResultAsync(new LabResultFormViewModel { OrderId = orderId, NumericValue = 9.8m, Comment = "Repeat in 24h" });

        using var db = _ctx.Fresh();
        var order = await db.ClinicalOrders
            .Include(o => o.LabDetail!).ThenInclude(d => d.Result)
            .Include(o => o.LabDetail!).ThenInclude(d => d.Specimen)
            .SingleAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.NotNull(order.CompletedAt);
        Assert.StartsWith("ACC-", order.LabDetail!.Specimen!.AccessionNumber);
        Assert.Equal(ResultFlag.Low, order.LabDetail.Result!.Flag);
        Assert.Equal(9.8m, order.LabDetail.Result.NumericValue);
    }

    [Fact]
    public async Task Second_result_is_rejected()
    {
        var orderId = await AcceptedOrderWithSpecimenAsync();
        await _ctx.Lab.EnterResultAsync(new LabResultFormViewModel { OrderId = orderId, NumericValue = 13m });

        await Assert.ThrowsAsync<ResultAlreadyRecordedException>(() =>
            _ctx.Lab.EnterResultAsync(new LabResultFormViewModel { OrderId = orderId, NumericValue = 14m }));
    }

    [Fact]
    public async Task Result_without_specimen_is_rejected()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var orderId = await _ctx.Orders.PlaceLabOrderAsync(new LabOrderFormViewModel
        {
            AdmissionId = admitted.AdmissionId,
            OrderingPractitionerId = admitted.Physician.Id,
            LabTestDefinitionId = _ctx.Data.NumericLabTest().Id
        });
        await _ctx.Lab.AcceptAsync(orderId);

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            _ctx.Lab.EnterResultAsync(new LabResultFormViewModel { OrderId = orderId, NumericValue = 13m }));
    }

    [Fact]
    public async Task Specimen_before_acceptance_is_an_invalid_transition()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var orderId = await _ctx.Orders.PlaceLabOrderAsync(new LabOrderFormViewModel
        {
            AdmissionId = admitted.AdmissionId,
            OrderingPractitionerId = admitted.Physician.Id,
            LabTestDefinitionId = _ctx.Data.NumericLabTest().Id
        });

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() =>
            _ctx.Lab.RecordSpecimenAsync(new SpecimenFormViewModel { OrderId = orderId, CollectedAt = _ctx.Clock.Now }));
    }

    [Fact]
    public void Text_test_flags_unexpected_value_as_abnormal()
    {
        var test = new LabTestDefinition { ResultType = ResultValueType.Text, ReferenceText = "Negative" };

        Assert.Equal(ResultFlag.Normal, test.Evaluate(null, " negative "));
        Assert.Equal(ResultFlag.Abnormal, test.Evaluate(null, "Positive"));
    }
}
