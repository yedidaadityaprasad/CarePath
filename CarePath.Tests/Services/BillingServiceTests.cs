using CarePath.Exceptions;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Services;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class BillingServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Theory]
    [InlineData("2026-03-01 10:00", "2026-03-01 18:00", 1)]
    [InlineData("2026-03-01 23:30", "2026-03-02 00:30", 1)]
    [InlineData("2026-03-01 10:00", "2026-03-04 09:00", 3)]
    public void Room_charge_counts_calendar_days_with_one_day_minimum(string admitted, string discharged, int expectedDays)
    {
        var (days, amount) = BillingService.CalculateRoomCharge(250m, DateTime.Parse(admitted), DateTime.Parse(discharged));

        Assert.Equal(expectedDays, days);
        Assert.Equal(250m * expectedDays, amount);
    }

    [Fact]
    public async Task Discharge_invoice_includes_room_lab_and_dispensed_medication_with_coverage()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx, dailyRate: 200m, coveragePercent: 80m, admittedAt: _ctx.Clock.Now.AddDays(-3));
        var test = _ctx.Data.NumericLabTest(price: 30m);
        var labId = await _ctx.Orders.PlaceLabOrderAsync(new LabOrderFormViewModel
        {
            AdmissionId = admitted.AdmissionId,
            OrderingPractitionerId = admitted.Physician.Id,
            LabTestDefinitionId = test.Id
        });
        await _ctx.Lab.AcceptAsync(labId);
        await _ctx.Lab.RecordSpecimenAsync(new SpecimenFormViewModel { OrderId = labId, CollectedAt = _ctx.Clock.Now });
        await _ctx.Lab.EnterResultAsync(new LabResultFormViewModel { OrderId = labId, NumericValue = 14m });

        var drug = _ctx.Data.Drug("Ondansetron", 5m);
        _ctx.Data.Stock(admitted.Ward, drug, 20);
        var medId = await _ctx.Orders.PlaceMedicationOrderAsync(new MedicationOrderFormViewModel
        {
            AdmissionId = admitted.AdmissionId,
            OrderingPractitionerId = admitted.Physician.Id,
            DrugId = drug.Id,
            Dose = "4 mg",
            Route = "Oral",
            Frequency = "PRN",
            Quantity = 4
        });
        await _ctx.Pharmacy.DispenseAsync(medId, null);

        var invoiceId = await _ctx.Admissions.DischargeAsync(_ctx.Data.DischargeForm(admitted.AdmissionId));

        using var db = _ctx.Fresh();
        var invoice = await db.Invoices.Include(i => i.Lines).Include(i => i.CoverageSnapshot).SingleAsync(i => i.Id == invoiceId);
        Assert.Equal(3, invoice.RoomDays);
        Assert.Equal(600m, invoice.Lines.Single(l => l.LineType == InvoiceLineType.RoomCharge).Amount);
        Assert.Equal(30m, invoice.Lines.Single(l => l.LineType == InvoiceLineType.LabCharge).Amount);
        Assert.Equal(20m, invoice.Lines.Single(l => l.LineType == InvoiceLineType.MedicationCharge).Amount);
        Assert.Equal(650m, invoice.SubTotal);
        Assert.Equal(520m, invoice.CoveredAmount);
        Assert.Equal(130m, invoice.PatientResponsibility);
        Assert.Equal(80m, invoice.CoverageSnapshot!.CoveragePercent);
        Assert.Equal(admitted.Coverage.MemberId, invoice.CoverageSnapshot.MemberId);
    }

    private async Task<int> DraftInvoiceAsync(decimal coveragePercent = 80m)
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx, dailyRate: 100m, coveragePercent: coveragePercent);
        return await _ctx.Admissions.DischargeAsync(_ctx.Data.DischargeForm(admitted.AdmissionId));
    }

    [Fact]
    public async Task Issue_then_pay_in_full_marks_invoice_paid()
    {
        var invoiceId = await DraftInvoiceAsync();
        await _ctx.Billing.IssueAsync(invoiceId, 30);
        var form = await _ctx.Billing.GetPaymentFormAsync(invoiceId);
        Assert.Equal(40m, form.BalanceDue);

        await _ctx.Billing.RecordPaymentAsync(new PaymentViewModel { InvoiceId = invoiceId, Amount = 15m, Method = PaymentMethod.Card });
        await _ctx.Billing.RecordPaymentAsync(new PaymentViewModel { InvoiceId = invoiceId, Amount = 25m, Method = PaymentMethod.Cash });

        using var db = _ctx.Fresh();
        var invoice = await db.Invoices.Include(i => i.Payments).SingleAsync(i => i.Id == invoiceId);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(0m, invoice.BalanceDue);
        Assert.Equal(_ctx.Clock.Now.Date.AddDays(30), invoice.DueDate);
    }

    [Fact]
    public async Task Issuing_fully_covered_invoice_goes_straight_to_paid()
    {
        var invoiceId = await DraftInvoiceAsync(coveragePercent: 100m);

        await _ctx.Billing.IssueAsync(invoiceId, 14);

        Assert.Equal(InvoiceStatus.Paid, (await _ctx.Fresh().Invoices.SingleAsync(i => i.Id == invoiceId)).Status);
    }

    [Fact]
    public async Task Payment_on_draft_invoice_is_an_invalid_transition()
    {
        var invoiceId = await DraftInvoiceAsync();

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() =>
            _ctx.Billing.RecordPaymentAsync(new PaymentViewModel { InvoiceId = invoiceId, Amount = 10m, Method = PaymentMethod.Cash }));
    }

    [Fact]
    public async Task Overpayment_is_rejected()
    {
        var invoiceId = await DraftInvoiceAsync();
        await _ctx.Billing.IssueAsync(invoiceId, 30);

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() =>
            _ctx.Billing.RecordPaymentAsync(new PaymentViewModel { InvoiceId = invoiceId, Amount = 40.01m, Method = PaymentMethod.Cash }));
        Assert.Equal(nameof(PaymentViewModel.Amount), ex.PropertyName);
    }

    [Fact]
    public async Task Void_is_rejected_once_payments_exist()
    {
        var invoiceId = await DraftInvoiceAsync();
        await _ctx.Billing.IssueAsync(invoiceId, 30);
        await _ctx.Billing.RecordPaymentAsync(new PaymentViewModel { InvoiceId = invoiceId, Amount = 10m, Method = PaymentMethod.Cash });

        await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Billing.VoidAsync(invoiceId, "Billed in error"));
    }

    [Fact]
    public async Task Void_draft_invoice_records_reason()
    {
        var invoiceId = await DraftInvoiceAsync();

        await _ctx.Billing.VoidAsync(invoiceId, "Billed in error");

        var invoice = await _ctx.Fresh().Invoices.Include(i => i.StatusChanges).SingleAsync(i => i.Id == invoiceId);
        Assert.Equal(InvoiceStatus.Void, invoice.Status);
        Assert.Equal("Billed in error", invoice.VoidReason);
        Assert.Single(invoice.StatusChanges, c => c.ToStatus == InvoiceStatus.Void);
    }
}
