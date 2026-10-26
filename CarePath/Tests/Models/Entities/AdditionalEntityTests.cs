using System;
using System.Collections.Generic;
using Xunit;
using CarePath.Models.Entities;

namespace CarePath.Tests.Models.Entities;

public class EnumsTests
{
    // ── Gender ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Gender_HasFourValues()
    {
        var values = Enum.GetValues<Gender>();
        Assert.Equal(4, values.Length);
    }

    [Theory]
    [InlineData(Gender.Female)]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Other)]
    [InlineData(Gender.Unknown)]
    public void Gender_AllValuesAreDefined(Gender gender)
    {
        Assert.True(Enum.IsDefined(gender));
    }

    // ── AllergySeverity ────────────────────────────────────────────────────────

    [Fact]
    public void AllergySeverity_HasThreeValues()
    {
        var values = Enum.GetValues<AllergySeverity>();
        Assert.Equal(3, values.Length);
    }

    [Theory]
    [InlineData(AllergySeverity.Mild)]
    [InlineData(AllergySeverity.Moderate)]
    [InlineData(AllergySeverity.Severe)]
    public void AllergySeverity_AllValuesAreDefined(AllergySeverity severity)
    {
        Assert.True(Enum.IsDefined(severity));
    }

    // ── PractitionerType ───────────────────────────────────────────────────────

    [Fact]
    public void PractitionerType_HasTwoValues()
    {
        var values = Enum.GetValues<PractitionerType>();
        Assert.Equal(2, values.Length);
    }

    // ── BedStatus ──────────────────────────────────────────────────────────────

    [Fact]
    public void BedStatus_HasThreeValues()
    {
        var values = Enum.GetValues<BedStatus>();
        Assert.Equal(3, values.Length);
    }

    // ── AdmissionStatus ────────────────────────────────────────────────────────

    [Fact]
    public void AdmissionStatus_HasTwoValues()
    {
        var values = Enum.GetValues<AdmissionStatus>();
        Assert.Equal(2, values.Length);
    }

    // ── OrderStatus ────────────────────────────────────────────────────────────

    [Fact]
    public void OrderStatus_HasFourValues()
    {
        var values = Enum.GetValues<OrderStatus>();
        Assert.Equal(4, values.Length);
    }

    // ── InvoiceStatus ──────────────────────────────────────────────────────────

    [Fact]
    public void InvoiceStatus_HasFourValues()
    {
        var values = Enum.GetValues<InvoiceStatus>();
        Assert.Equal(4, values.Length);
    }

    // ── ResultFlag ─────────────────────────────────────────────────────────────

    [Fact]
    public void ResultFlag_HasFourValues()
    {
        var values = Enum.GetValues<ResultFlag>();
        Assert.Equal(4, values.Length);
    }

    // ── StockMovementType ──────────────────────────────────────────────────────

    [Fact]
    public void StockMovementType_HasThreeValues()
    {
        var values = Enum.GetValues<StockMovementType>();
        Assert.Equal(3, values.Length);
    }

    // ── PaymentMethod ──────────────────────────────────────────────────────────

    [Fact]
    public void PaymentMethod_HasFourValues()
    {
        var values = Enum.GetValues<PaymentMethod>();
        Assert.Equal(4, values.Length);
    }

    // ── InvoiceLineType ────────────────────────────────────────────────────────

    [Fact]
    public void InvoiceLineType_HasThreeValues()
    {
        var values = Enum.GetValues<InvoiceLineType>();
        Assert.Equal(3, values.Length);
    }
}

public class LabResultTests
{
    [Fact]
    public void DisplayValue_NumericValue_ReturnsFormattedNumber()
    {
        var result = new LabResult { NumericValue = 7.5m };
        Assert.Equal("7.5", result.DisplayValue);
    }

    [Fact]
    public void DisplayValue_NumericValueWithMoreDecimals_FormatsCorrectly()
    {
        var result = new LabResult { NumericValue = 7.123m };
        Assert.Equal("7.123", result.DisplayValue);
    }

    [Fact]
    public void DisplayValue_TextValue_ReturnsText()
    {
        var result = new LabResult { TextValue = "Positive" };
        Assert.Equal("Positive", result.DisplayValue);
    }

    [Fact]
    public void DisplayValue_BothNull_ReturnsEmptyString()
    {
        var result = new LabResult { NumericValue = null, TextValue = null };
        Assert.Equal(string.Empty, result.DisplayValue);
    }

    [Fact]
    public void DisplayValue_NumericTakesPrecedenceOverText()
    {
        var result = new LabResult { NumericValue = 5.0m, TextValue = "SomeText" };
        Assert.Equal("5", result.DisplayValue);
    }

    [Fact]
    public void Flag_DefaultsToNormal()
    {
        var result = new LabResult();
        Assert.Equal(ResultFlag.Normal, result.Flag);
    }
}

public class PaymentTests
{
    [Fact]
    public void Payment_DefaultProperties_AreCorrect()
    {
        var payment = new Payment();
        Assert.Equal(0m, payment.Amount);
        Assert.Equal(string.Empty, payment.ReceivedBy);
    }

    [Fact]
    public void Payment_CanSetAllProperties()
    {
        var payment = new Payment
        {
            Id = 1,
            InvoiceId = 10,
            Amount = 500m,
            Method = PaymentMethod.Card,
            Reference = "TXN-001",
            ReceivedAt = new DateTime(2024, 5, 1),
            ReceivedBy = "cashier"
        };
        Assert.Equal(500m, payment.Amount);
        Assert.Equal(PaymentMethod.Card, payment.Method);
        Assert.Equal("TXN-001", payment.Reference);
    }
}

public class VitalSignTests
{
    [Fact]
    public void VitalSign_DefaultProperties_AreCorrect()
    {
        var vital = new VitalSign();
        Assert.Equal(0m, vital.TemperatureCelsius);
        Assert.Equal(0, vital.PulseBpm);
        Assert.Equal(0, vital.SystolicBp);
        Assert.Equal(0, vital.DiastolicBp);
        Assert.Equal(string.Empty, vital.RecordedBy);
    }

    [Fact]
    public void VitalSign_CanSetAllProperties()
    {
        var vital = new VitalSign
        {
            Id = 1,
            AdmissionId = 5,
            RecordedAt = new DateTime(2024, 5, 1, 8, 0, 0),
            TemperatureCelsius = 37.2m,
            PulseBpm = 72,
            SystolicBp = 120,
            DiastolicBp = 80,
            RespiratoryRate = 16,
            OxygenSaturation = 98,
            RecordedBy = "nurse1"
        };
        Assert.Equal(37.2m, vital.TemperatureCelsius);
        Assert.Equal(72, vital.PulseBpm);
        Assert.Equal(120, vital.SystolicBp);
        Assert.Equal(80, vital.DiastolicBp);
    }
}

public class AdmissionNoteTests
{
    [Fact]
    public void AdmissionNote_DefaultProperties_AreCorrect()
    {
        var note = new AdmissionNote();
        Assert.Equal(string.Empty, note.Text);
        Assert.Equal(string.Empty, note.Author);
    }

    [Fact]
    public void AdmissionNote_CanSetAllProperties()
    {
        var note = new AdmissionNote
        {
            Id = 1,
            AdmissionId = 5,
            Text = "Patient is stable",
            WrittenAt = new DateTime(2024, 5, 1),
            Author = "dr.jones"
        };
        Assert.Equal("Patient is stable", note.Text);
        Assert.Equal("dr.jones", note.Author);
    }
}

public class DischargeSummaryTests
{
    [Fact]
    public void DischargeSummary_DefaultProperties_AreCorrect()
    {
        var summary = new DischargeSummary();
        Assert.Equal(string.Empty, summary.Disposition);
        Assert.Equal(string.Empty, summary.FinalDiagnosis);
        Assert.Equal(string.Empty, summary.Summary);
        Assert.Null(summary.FollowUpInstructions);
    }

    [Fact]
    public void DischargeSummary_CanSetAllProperties()
    {
        var summary = new DischargeSummary
        {
            Id = 1,
            AdmissionId = 5,
            Disposition = "Home",
            FinalDiagnosis = "Pneumonia",
            Summary = "Patient recovered well",
            FollowUpInstructions = "Follow up in 2 weeks",
            WrittenAt = new DateTime(2024, 5, 10),
            WrittenBy = "dr.smith"
        };
        Assert.Equal("Home", summary.Disposition);
        Assert.Equal("Pneumonia", summary.FinalDiagnosis);
        Assert.Equal("Follow up in 2 weeks", summary.FollowUpInstructions);
    }
}

public class InvoiceLineTests
{
    [Fact]
    public void InvoiceLine_DefaultProperties_AreCorrect()
    {
        var line = new InvoiceLine();
        Assert.Equal(string.Empty, line.Description);
        Assert.Equal(0m, line.Quantity);
        Assert.Equal(0m, line.UnitPrice);
        Assert.Equal(0m, line.Amount);
        Assert.Null(line.ClinicalOrderId);
    }

    [Fact]
    public void InvoiceLine_CanSetAllProperties()
    {
        var line = new InvoiceLine
        {
            Id = 1,
            InvoiceId = 10,
            LineType = InvoiceLineType.LabCharge,
            Description = "CBC Test",
            Quantity = 1m,
            UnitPrice = 150m,
            Amount = 150m
        };
        Assert.Equal(InvoiceLineType.LabCharge, line.LineType);
        Assert.Equal(150m, line.Amount);
    }
}

public class StockMovementTests
{
    [Fact]
    public void StockMovement_DefaultProperties_AreCorrect()
    {
        var movement = new StockMovement();
        Assert.Equal(0, movement.QuantityChange);
        Assert.Equal(0, movement.QuantityAfter);
        Assert.Equal(string.Empty, movement.PerformedBy);
        Assert.Equal(string.Empty, movement.Reference);
    }

    [Fact]
    public void StockMovement_CanSetAllProperties()
    {
        var movement = new StockMovement
        {
            Id = 1,
            WardStockId = 5,
            Type = StockMovementType.Restock,
            QuantityChange = 50,
            QuantityAfter = 100,
            OccurredAt = new DateTime(2024, 5, 1),
            PerformedBy = "pharmacist",
            Reference = "PO-001"
        };
        Assert.Equal(StockMovementType.Restock, movement.Type);
        Assert.Equal(50, movement.QuantityChange);
        Assert.Equal(100, movement.QuantityAfter);
    }
}

public class PatientAllergyTests
{
    [Fact]
    public void PatientAllergy_DefaultProperties_AreCorrect()
    {
        var allergy = new PatientAllergy();
        Assert.Equal(string.Empty, allergy.Substance);
        Assert.Null(allergy.Reaction);
        Assert.Equal(string.Empty, allergy.RecordedBy);
    }

    [Fact]
    public void PatientAllergy_CanSetAllProperties()
    {
        var allergy = new PatientAllergy
        {
            Id = 1,
            PatientId = 5,
            Substance = "Penicillin",
            Reaction = "Rash",
            Severity = AllergySeverity.Moderate,
            RecordedAt = new DateTime(2024, 5, 1),
            RecordedBy = "nurse1"
        };
        Assert.Equal("Penicillin", allergy.Substance);
        Assert.Equal(AllergySeverity.Moderate, allergy.Severity);
    }
}

public class EmergencyContactTests
{
    [Fact]
    public void EmergencyContact_DefaultProperties_AreCorrect()
    {
        var contact = new EmergencyContact();
        Assert.Equal(string.Empty, contact.Name);
        Assert.Equal(string.Empty, contact.Relationship);
        Assert.Equal(string.Empty, contact.Phone);
        Assert.False(contact.IsPrimary);
    }

    [Fact]
    public void EmergencyContact_CanSetAllProperties()
    {
        var contact = new EmergencyContact
        {
            Id = 1,
            PatientId = 5,
            Name = "Jane Doe",
            Relationship = "Spouse",
            Phone = "555-1234",
            IsPrimary = true
        };
        Assert.Equal("Jane Doe", contact.Name);
        Assert.Equal("Spouse", contact.Relationship);
        Assert.True(contact.IsPrimary);
    }
}

public class DepartmentTests
{
    [Fact]
    public void Department_DefaultProperties_AreCorrect()
    {
        var dept = new Department();
        Assert.Equal(string.Empty, dept.Code);
        Assert.Equal(string.Empty, dept.Name);
        Assert.NotNull(dept.Wards);
        Assert.Empty(dept.Wards);
    }

    [Fact]
    public void Department_CanSetProperties()
    {
        var dept = new Department { Id = 1, Code = "CARD", Name = "Cardiology" };
        Assert.Equal("CARD", dept.Code);
        Assert.Equal("Cardiology", dept.Name);
    }
}

public class SpecialtyTests
{
    [Fact]
    public void Specialty_DefaultProperties_AreCorrect()
    {
        var specialty = new Specialty();
        Assert.Equal(string.Empty, specialty.Code);
        Assert.Equal(string.Empty, specialty.Name);
        Assert.NotNull(specialty.Practitioners);
    }

    [Fact]
    public void Specialty_CanSetProperties()
    {
        var specialty = new Specialty { Id = 1, Code = "CARD", Name = "Cardiology" };
        Assert.Equal("CARD", specialty.Code);
    }
}

public class DrugCategoryTests
{
    [Fact]
    public void DrugCategory_DefaultProperties_AreCorrect()
    {
        var category = new DrugCategory();
        Assert.Equal(string.Empty, category.Name);
        Assert.NotNull(category.Drugs);
        Assert.Empty(category.Drugs);
    }

    [Fact]
    public void DrugCategory_CanSetName()
    {
        var category = new DrugCategory { Id = 1, Name = "Antibiotics" };
        Assert.Equal("Antibiotics", category.Name);
    }
}

public class ApplicationUserTests
{
    [Fact]
    public void ApplicationUser_DefaultProperties_AreCorrect()
    {
        var user = new ApplicationUser();
        Assert.Equal(string.Empty, user.FullName);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void ApplicationUser_CanSetProperties()
    {
        var user = new ApplicationUser
        {
            FullName = "John Admin",
            IsActive = false,
            CreatedAt = new DateTime(2024, 1, 1)
        };
        Assert.Equal("John Admin", user.FullName);
        Assert.False(user.IsActive);
    }
}

public class AuditEntryTests
{
    [Fact]
    public void AuditEntry_DefaultProperties_AreCorrect()
    {
        var entry = new AuditEntry();
        Assert.Equal(string.Empty, entry.UserName);
        Assert.Equal(string.Empty, entry.Action);
        Assert.Equal(string.Empty, entry.EntityName);
        Assert.Equal(string.Empty, entry.EntityId);
        Assert.Null(entry.Details);
    }

    [Fact]
    public void AuditEntry_CanSetAllProperties()
    {
        var entry = new AuditEntry
        {
            Id = 1,
            UserName = "admin",
            Action = "Create",
            EntityName = "Patient",
            EntityId = "42",
            Timestamp = new DateTime(2024, 5, 1),
            Details = "Created patient record"
        };
        Assert.Equal("admin", entry.UserName);
        Assert.Equal("Create", entry.Action);
        Assert.Equal("Patient", entry.EntityName);
        Assert.Equal("42", entry.EntityId);
        Assert.Equal("Created patient record", entry.Details);
    }
}

public class PayerTests
{
    [Fact]
    public void Payer_DefaultProperties_AreCorrect()
    {
        var payer = new Payer();
        Assert.Equal(string.Empty, payer.Code);
        Assert.Equal(string.Empty, payer.Name);
        Assert.Null(payer.Phone);
        Assert.Null(payer.ClaimsEmail);
        Assert.True(payer.IsActive);
        Assert.NotNull(payer.Coverages);
    }

    [Fact]
    public void Payer_CanSetAllProperties()
    {
        var payer = new Payer
        {
            Id = 1,
            Code = "BC",
            Name = "BlueCross",
            Phone = "800-555-1234",
            ClaimsEmail = "claims@bluecross.com",
            DefaultCoveragePercent = 80m,
            IsActive = true
        };
        Assert.Equal("BC", payer.Code);
        Assert.Equal("BlueCross", payer.Name);
        Assert.Equal(80m, payer.DefaultCoveragePercent);
    }
}

public class MedicationOrderDetailTests
{
    [Fact]
    public void MedicationOrderDetail_DefaultProperties_AreCorrect()
    {
        var detail = new MedicationOrderDetail();
        Assert.Equal(string.Empty, detail.Dose);
        Assert.Equal(string.Empty, detail.Route);
        Assert.Equal(string.Empty, detail.Frequency);
        Assert.Equal(0, detail.Quantity);
    }

    [Fact]
    public void MedicationOrderDetail_CanSetAllProperties()
    {
        var detail = new MedicationOrderDetail
        {
            Id = 1,
            ClinicalOrderId = 5,
            DrugId = 10,
            Dose = "500mg",
            Route = "Oral",
            Frequency = "TID",
            Quantity = 3
        };
        Assert.Equal("500mg", detail.Dose);
        Assert.Equal("Oral", detail.Route);
        Assert.Equal("TID", detail.Frequency);
        Assert.Equal(3, detail.Quantity);
    }
}

public class BedAssignmentTests
{
    [Fact]
    public void BedAssignment_DefaultProperties_AreCorrect()
    {
        var assignment = new BedAssignment();
        Assert.Equal(string.Empty, assignment.AssignedBy);
        Assert.Equal(string.Empty, assignment.Reason);
    }
}

public class AdmissionDiagnosisTests
{
    [Fact]
    public void AdmissionDiagnosis_DefaultProperties_AreCorrect()
    {
        var diagnosis = new AdmissionDiagnosis();
        Assert.Equal(string.Empty, diagnosis.Code);
        Assert.Equal(string.Empty, diagnosis.Description);
    }
}
