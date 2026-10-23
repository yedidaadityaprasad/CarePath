using CarePath.Exceptions;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class PatientServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private static PatientFormViewModel NewPatient(string mrn) => new()
    {
        MedicalRecordNumber = mrn,
        FirstName = "Meera",
        LastName = "Iyer",
        DateOfBirth = new DateTime(1975, 2, 3),
        Gender = Gender.Female,
        AddressLine1 = "12 Lake Road",
        City = "Pune",
        PostalCode = "411001",
        Country = "India",
        ContactName = "Arjun Iyer",
        ContactRelationship = "Spouse",
        ContactPhone = "+91 98200 00000"
    };

    [Fact]
    public async Task Register_normalises_mrn_saves_address_contact_and_audits()
    {
        var id = await _ctx.Patients.RegisterAsync(NewPatient("  mrn-7001 "));

        using var db = _ctx.Fresh();
        var patient = await db.Patients.Include(p => p.Address).Include(p => p.EmergencyContacts).SingleAsync(p => p.Id == id);
        Assert.Equal("MRN-7001", patient.MedicalRecordNumber);
        Assert.True(patient.IsActive);
        Assert.Equal("Pune", patient.Address!.City);
        Assert.Single(patient.EmergencyContacts, c => c.IsPrimary && c.Name == "Arjun Iyer");
        Assert.True(await db.AuditEntries.AnyAsync(a => a.EntityName == nameof(Patient) && a.EntityId == id.ToString()));
    }

    [Fact]
    public async Task Register_rejects_duplicate_mrn_case_insensitively()
    {
        await _ctx.Patients.RegisterAsync(NewPatient("MRN-7002"));

        var ex = await Assert.ThrowsAsync<DuplicateValueException>(() => _ctx.Patients.RegisterAsync(NewPatient("mrn-7002")));
        Assert.Equal(nameof(PatientFormViewModel.MedicalRecordNumber), ex.PropertyName);
    }

    [Fact]
    public async Task Register_rejects_future_date_of_birth()
    {
        var form = NewPatient("MRN-7003");
        form.DateOfBirth = _ctx.Clock.Now.AddDays(1);

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Patients.RegisterAsync(form));
        Assert.Equal(nameof(PatientFormViewModel.DateOfBirth), ex.PropertyName);
    }

    [Fact]
    public async Task Update_cannot_change_mrn()
    {
        var id = await _ctx.Patients.RegisterAsync(NewPatient("MRN-7004"));
        var form = await _ctx.Patients.GetForEditAsync(id);
        form.MedicalRecordNumber = "MRN-9999";

        await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Patients.UpdateAsync(form));
    }

    [Fact]
    public async Task Deactivate_cancels_future_appointments()
    {
        var patient = _ctx.Data.Patient();
        var practitioner = _ctx.Data.Practitioner();
        var appointmentId = await _ctx.Appointments.BookAsync(new AppointmentFormViewModel
        {
            PatientId = patient.Id,
            PractitionerId = practitioner.Id,
            StartsAt = _ctx.Clock.Now.AddDays(3),
            DurationMinutes = 30,
            Reason = "Follow-up"
        });

        await _ctx.Patients.DeactivateAsync(patient.Id, "Moved out of area");

        using var db = _ctx.Fresh();
        var saved = await db.Patients.SingleAsync(p => p.Id == patient.Id);
        Assert.False(saved.IsActive);
        Assert.Equal("Moved out of area", saved.DeactivationReason);
        var appointment = await db.Appointments.SingleAsync(a => a.Id == appointmentId);
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public async Task Deactivate_is_rejected_while_patient_is_admitted()
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);

        await Assert.ThrowsAsync<DomainValidationException>(() => _ctx.Patients.DeactivateAsync(admitted.Patient.Id, "Duplicate record"));
        Assert.True((await _ctx.Fresh().Patients.SingleAsync(p => p.Id == admitted.Patient.Id)).IsActive);
    }

    [Fact]
    public async Task AddAllergy_rejects_duplicate_substance()
    {
        var patient = _ctx.Data.Patient();
        await _ctx.Patients.AddAllergyAsync(new AllergyFormViewModel { PatientId = patient.Id, Substance = "Penicillin", Severity = AllergySeverity.Severe });

        await Assert.ThrowsAsync<DuplicateValueException>(() =>
            _ctx.Patients.AddAllergyAsync(new AllergyFormViewModel { PatientId = patient.Id, Substance = "penicillin" }));
    }

    [Fact]
    public async Task Reactivate_active_patient_is_an_invalid_transition()
    {
        var patient = _ctx.Data.Patient();
        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() => _ctx.Patients.ReactivateAsync(patient.Id));
    }
}
