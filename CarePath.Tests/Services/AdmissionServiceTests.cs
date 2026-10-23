using CarePath.Exceptions;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class AdmissionServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public async Task Admit_occupies_bed_records_assignment_diagnosis_and_audit()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);

        using var db = _ctx.Fresh();
        var admission = await db.Admissions
            .Include(a => a.BedAssignments)
            .Include(a => a.Diagnoses)
            .SingleAsync(a => a.Id == admitted.AdmissionId);
        Assert.Equal(AdmissionStatus.Open, admission.Status);
        Assert.StartsWith("ADM-", admission.AdmissionNumber);
        Assert.Equal(admitted.Coverage.Id, admission.CoverageId);
        Assert.Single(admission.BedAssignments);
        Assert.Equal("J18.9", Assert.Single(admission.Diagnoses).Code);
        Assert.Equal(BedStatus.Occupied, (await db.Beds.SingleAsync(b => b.Id == admitted.Bed.Id)).Status);
        Assert.True(await db.AuditEntries.AnyAsync(e => e.EntityName == nameof(Admission) && e.EntityId == admitted.AdmissionId.ToString()));
    }

    [Fact]
    public async Task Admit_without_active_coverage_is_rejected_and_bed_stays_free()
    {
        var patient = _ctx.Data.Patient();
        var bed = _ctx.Data.Bed(_ctx.Data.Ward());

        await Assert.ThrowsAsync<CoverageRequiredException>(() =>
            _ctx.Admissions.AdmitAsync(_ctx.Data.AdmitForm(patient, bed, _ctx.Data.Practitioner())));
        Assert.Equal(BedStatus.Available, (await _ctx.Fresh().Beds.SingleAsync(b => b.Id == bed.Id)).Status);
    }

    [Fact]
    public async Task Admit_into_occupied_bed_is_rejected()
    {
        var first = await _ctx.Data.AdmitAsync(_ctx);
        var patient = _ctx.Data.Patient();
        _ctx.Data.Coverage(patient, _ctx.Data.Payer());

        await Assert.ThrowsAsync<BedUnavailableException>(() =>
            _ctx.Admissions.AdmitAsync(_ctx.Data.AdmitForm(patient, first.Bed, first.Physician)));
    }

    [Fact]
    public async Task Admit_with_nurse_as_attending_is_rejected()
    {
        var patient = _ctx.Data.Patient();
        _ctx.Data.Coverage(patient, _ctx.Data.Payer());
        var nurse = _ctx.Data.Practitioner(PractitionerType.Nurse);

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() =>
            _ctx.Admissions.AdmitAsync(_ctx.Data.AdmitForm(patient, _ctx.Data.Bed(_ctx.Data.Ward()), nurse)));
        Assert.Equal(nameof(AdmitPatientViewModel.AttendingPractitionerId), ex.PropertyName);
    }

    [Fact]
    public async Task Transfer_moves_patient_and_frees_previous_bed()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var newBed = _ctx.Data.Bed(_ctx.Data.Ward());

        await _ctx.Admissions.TransferAsync(new TransferViewModel { AdmissionId = admitted.AdmissionId, NewBedId = newBed.Id, Reason = "Step-down care" });

        using var db = _ctx.Fresh();
        var admission = await db.Admissions.Include(a => a.BedAssignments).SingleAsync(a => a.Id == admitted.AdmissionId);
        Assert.Equal(newBed.Id, admission.CurrentBedId);
        Assert.Equal(2, admission.BedAssignments.Count);
        Assert.Single(admission.BedAssignments, b => b.ReleasedAt == null && b.BedId == newBed.Id);
        Assert.Equal(BedStatus.Available, (await db.Beds.SingleAsync(b => b.Id == admitted.Bed.Id)).Status);
        Assert.Equal(BedStatus.Occupied, (await db.Beds.SingleAsync(b => b.Id == newBed.Id)).Status);
    }

    [Fact]
    public async Task Discharge_frees_bed_cancels_placed_orders_and_creates_draft_invoice()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var orderId = await _ctx.Orders.PlaceLabOrderAsync(new LabOrderFormViewModel
        {
            AdmissionId = admitted.AdmissionId,
            OrderingPractitionerId = admitted.Physician.Id,
            LabTestDefinitionId = _ctx.Data.NumericLabTest().Id
        });

        var invoiceId = await _ctx.Admissions.DischargeAsync(_ctx.Data.DischargeForm(admitted.AdmissionId));

        using var db = _ctx.Fresh();
        var admission = await db.Admissions.Include(a => a.DischargeSummary).SingleAsync(a => a.Id == admitted.AdmissionId);
        Assert.Equal(AdmissionStatus.Discharged, admission.Status);
        Assert.Equal("Home", admission.DischargeSummary!.Disposition);
        Assert.Equal(BedStatus.Available, (await db.Beds.SingleAsync(b => b.Id == admitted.Bed.Id)).Status);
        Assert.Equal(OrderStatus.Cancelled, (await db.ClinicalOrders.SingleAsync(o => o.Id == orderId)).Status);
        var invoice = await db.Invoices.SingleAsync(i => i.Id == invoiceId);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Equal(admitted.AdmissionId, invoice.AdmissionId);
    }

    [Fact]
    public async Task Discharge_is_rejected_while_an_order_is_in_progress()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var orderId = await _ctx.Orders.PlaceLabOrderAsync(new LabOrderFormViewModel
        {
            AdmissionId = admitted.AdmissionId,
            OrderingPractitionerId = admitted.Physician.Id,
            LabTestDefinitionId = _ctx.Data.NumericLabTest().Id
        });
        await _ctx.Lab.AcceptAsync(orderId);

        await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Admissions.DischargeAsync(_ctx.Data.DischargeForm(admitted.AdmissionId)));

        using var db = _ctx.Fresh();
        Assert.Equal(AdmissionStatus.Open, (await db.Admissions.SingleAsync(a => a.Id == admitted.AdmissionId)).Status);
        Assert.False(await db.Invoices.AnyAsync());
    }

    [Fact]
    public async Task Second_discharge_is_an_invalid_transition()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        await _ctx.Admissions.DischargeAsync(_ctx.Data.DischargeForm(admitted.AdmissionId));

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() => _ctx.Admissions.DischargeAsync(_ctx.Data.DischargeForm(admitted.AdmissionId)));
    }

    [Fact]
    public async Task Vitals_reject_diastolic_not_below_systolic()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);

        await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Admissions.RecordVitalsAsync(new VitalsViewModel
        {
            AdmissionId = admitted.AdmissionId,
            TemperatureCelsius = 37.2m,
            PulseBpm = 80,
            SystolicBp = 90,
            DiastolicBp = 95,
            RespiratoryRate = 16,
            OxygenSaturation = 97
        }));
    }
}
