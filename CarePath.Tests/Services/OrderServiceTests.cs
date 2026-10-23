using CarePath.Exceptions;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class OrderServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private static LabOrderFormViewModel LabForm(AdmittedPatient admitted, LabTestDefinition test) => new()
    {
        AdmissionId = admitted.AdmissionId,
        OrderingPractitionerId = admitted.Physician.Id,
        LabTestDefinitionId = test.Id,
        Priority = OrderPriority.Urgent,
        ClinicalIndication = "Suspected anaemia"
    };

    private static MedicationOrderFormViewModel MedicationForm(AdmittedPatient admitted, Drug drug) => new()
    {
        AdmissionId = admitted.AdmissionId,
        OrderingPractitionerId = admitted.Physician.Id,
        DrugId = drug.Id,
        Dose = "1 g",
        Route = "IV",
        Frequency = "8 hourly",
        Quantity = 6
    };

    [Fact]
    public async Task Place_lab_order_creates_placed_order_with_detail()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var test = _ctx.Data.NumericLabTest();

        var id = await _ctx.Orders.PlaceLabOrderAsync(LabForm(admitted, test));

        var order = await _ctx.Fresh().ClinicalOrders.Include(o => o.LabDetail).SingleAsync(o => o.Id == id);
        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal(OrderType.Lab, order.Type);
        Assert.Equal(OrderPriority.Urgent, order.Priority);
        Assert.Equal(test.Id, order.LabDetail!.LabTestDefinitionId);
        Assert.StartsWith("ORD-", order.OrderNumber);
    }

    [Fact]
    public async Task Duplicate_pending_lab_test_is_rejected()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var test = _ctx.Data.NumericLabTest();
        await _ctx.Orders.PlaceLabOrderAsync(LabForm(admitted, test));

        await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Orders.PlaceLabOrderAsync(LabForm(admitted, test)));
    }

    [Fact]
    public async Task Medication_matching_recorded_allergy_is_rejected()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        await _ctx.Patients.AddAllergyAsync(new AllergyFormViewModel { PatientId = admitted.Patient.Id, Substance = "Amoxicillin", Severity = AllergySeverity.Severe });
        var drug = _ctx.Data.Drug("Amoxicillin");

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Orders.PlaceMedicationOrderAsync(MedicationForm(admitted, drug)));
        Assert.Equal(nameof(MedicationOrderFormViewModel.DrugId), ex.PropertyName);
    }

    [Fact]
    public async Task Ordering_on_discharged_admission_is_rejected()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        await _ctx.Admissions.DischargeAsync(_ctx.Data.DischargeForm(admitted.AdmissionId));

        await Assert.ThrowsAsync<AdmissionClosedException>(() => _ctx.Orders.PlaceMedicationOrderAsync(MedicationForm(admitted, _ctx.Data.Drug())));
    }

    [Fact]
    public async Task Cancel_after_lab_accepts_is_an_invalid_transition()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var id = await _ctx.Orders.PlaceLabOrderAsync(LabForm(admitted, _ctx.Data.NumericLabTest()));
        await _ctx.Lab.AcceptAsync(id);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() => _ctx.Orders.CancelAsync(id, "Not needed"));
    }

    [Fact]
    public async Task Edit_placed_medication_order_updates_detail()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var id = await _ctx.Orders.PlaceMedicationOrderAsync(MedicationForm(admitted, _ctx.Data.Drug()));
        var form = await _ctx.Orders.GetForEditAsync(id);
        form.Quantity = 10;
        form.Priority = OrderPriority.Stat;

        await _ctx.Orders.UpdateAsync(form);

        var order = await _ctx.Fresh().ClinicalOrders.Include(o => o.MedicationDetail).SingleAsync(o => o.Id == id);
        Assert.Equal(10, order.MedicationDetail!.Quantity);
        Assert.Equal(OrderPriority.Stat, order.Priority);
    }
}
