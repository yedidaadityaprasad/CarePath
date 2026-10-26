using System;
using System.Collections.Generic;
using Xunit;
using CarePath.Models.Entities;

namespace CarePath.Tests.Models.Entities;

public class PatientTests
{
    private static Patient CreatePatient(DateTime dob, string mrn = "MRN-001",
        string first = "John", string last = "Doe")
    {
        return new Patient
        {
            Id = 1,
            MedicalRecordNumber = mrn,
            FirstName = first,
            LastName = last,
            DateOfBirth = dob,
            Gender = Gender.Male,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow
        };
    }

    // ── FullName ───────────────────────────────────────────────────────────────

    [Fact]
    public void FullName_ReturnsLastNameCommaFirstName()
    {
        var patient = CreatePatient(new DateTime(1990, 1, 1), first: "Jane", last: "Smith");
        Assert.Equal("Smith, Jane", patient.FullName);
    }

    [Fact]
    public void FullName_WithEmptyNames_ReturnsComma()
    {
        var patient = new Patient { FirstName = "", LastName = "" };
        Assert.Equal(", ", patient.FullName);
    }

    // ── AgeOn ──────────────────────────────────────────────────────────────────

    [Fact]
    public void AgeOn_BirthdayNotYetReached_ReturnsAgeMinusOne()
    {
        // Born 1990-06-15; on 2024-06-14 they haven't had their birthday yet
        var patient = CreatePatient(new DateTime(1990, 6, 15));
        var age = patient.AgeOn(new DateTime(2024, 6, 14));
        Assert.Equal(33, age);
    }

    [Fact]
    public void AgeOn_BirthdayReached_ReturnsCorrectAge()
    {
        var patient = CreatePatient(new DateTime(1990, 6, 15));
        var age = patient.AgeOn(new DateTime(2024, 6, 15));
        Assert.Equal(34, age);
    }

    [Fact]
    public void AgeOn_BirthdayAfterDate_ReturnsAgeMinusOne()
    {
        var patient = CreatePatient(new DateTime(1990, 12, 31));
        var age = patient.AgeOn(new DateTime(2024, 1, 1));
        Assert.Equal(33, age);
    }

    [Fact]
    public void AgeOn_SameDayAsBirth_ReturnsZero()
    {
        var dob = new DateTime(2000, 5, 20);
        var patient = CreatePatient(dob);
        var age = patient.AgeOn(dob);
        Assert.Equal(0, age);
    }

    [Fact]
    public void AgeOn_NegativeAge_ReturnsZero()
    {
        // Date before birth
        var patient = CreatePatient(new DateTime(2000, 1, 1));
        var age = patient.AgeOn(new DateTime(1999, 1, 1));
        Assert.Equal(0, age);
    }

    // ── Properties ────────────────────────────────────────────────────────────

    [Fact]
    public void IsActive_DefaultsToTrue()
    {
        var patient = new Patient();
        Assert.True(patient.IsActive);
    }

    [Fact]
    public void Collections_DefaultToEmptyLists()
    {
        var patient = new Patient();
        Assert.NotNull(patient.EmergencyContacts);
        Assert.NotNull(patient.Allergies);
        Assert.NotNull(patient.Coverages);
        Assert.NotNull(patient.Admissions);
        Assert.NotNull(patient.Appointments);
    }

    [Fact]
    public void MedicalRecordNumber_DefaultsToEmptyString()
    {
        var patient = new Patient();
        Assert.Equal(string.Empty, patient.MedicalRecordNumber);
    }

    [Fact]
    public void Gender_CanBeSetToAllValues()
    {
        foreach (Gender g in Enum.GetValues<Gender>())
        {
            var patient = new Patient { Gender = g };
            Assert.Equal(g, patient.Gender);
        }
    }
}

public class AdmissionTests
{
    private static Admission CreateAdmission(AdmissionStatus status = AdmissionStatus.Open,
        DateTime? admittedAt = null, DateTime? dischargedAt = null)
    {
        return new Admission
        {
            Id = 1,
            AdmissionNumber = "ADM-001",
            Status = status,
            AdmittedAt = admittedAt ?? new DateTime(2024, 1, 1),
            DischargedAt = dischargedAt,
            ReasonForAdmission = "Fever",
            AdmittedBy = "admin"
        };
    }

    // ── IsOpen ─────────────────────────────────────────────────────────────────

    [Fact]
    public void IsOpen_WhenStatusIsOpen_ReturnsTrue()
    {
        var admission = CreateAdmission(AdmissionStatus.Open);
        Assert.True(admission.IsOpen);
    }

    [Fact]
    public void IsOpen_WhenStatusIsDischarged_ReturnsFalse()
    {
        var admission = CreateAdmission(AdmissionStatus.Discharged);
        Assert.False(admission.IsOpen);
    }

    // ── LengthOfStayDays ───────────────────────────────────────────────────────

    [Fact]
    public void LengthOfStayDays_SameDay_ReturnsOne()
    {
        var admission = CreateAdmission(admittedAt: new DateTime(2024, 3, 10));
        var los = admission.LengthOfStayDays(new DateTime(2024, 3, 10));
        Assert.Equal(1, los);
    }

    [Fact]
    public void LengthOfStayDays_FiveDays_ReturnsFive()
    {
        var admission = CreateAdmission(admittedAt: new DateTime(2024, 3, 1));
        var los = admission.LengthOfStayDays(new DateTime(2024, 3, 6));
        Assert.Equal(5, los);
    }

    [Fact]
    public void LengthOfStayDays_WithDischargeDate_UsesDischargeDate()
    {
        var admission = CreateAdmission(
            admittedAt: new DateTime(2024, 3, 1),
            dischargedAt: new DateTime(2024, 3, 4));
        var los = admission.LengthOfStayDays(new DateTime(2024, 3, 10));
        Assert.Equal(3, los);
    }

    [Fact]
    public void LengthOfStayDays_NeverLessThanOne()
    {
        var admission = CreateAdmission(admittedAt: new DateTime(2024, 3, 10));
        var los = admission.LengthOfStayDays(new DateTime(2024, 3, 9));
        Assert.Equal(1, los);
    }

    // ── Collections ───────────────────────────────────────────────────────────

    [Fact]
    public void Collections_DefaultToEmptyLists()
    {
        var admission = new Admission();
        Assert.NotNull(admission.BedAssignments);
        Assert.NotNull(admission.Diagnoses);
        Assert.NotNull(admission.Notes);
        Assert.NotNull(admission.Vitals);
        Assert.NotNull(admission.Orders);
    }

    [Fact]
    public void Status_DefaultsToOpen()
    {
        var admission = new Admission();
        Assert.Equal(AdmissionStatus.Open, admission.Status);
    }
}

public class BedTests
{
    private static Bed CreateBed(BedStatus status = BedStatus.Available, bool wardClosed = false)
    {
        return new Bed
        {
            Id = 1,
            BedNumber = "101",
            Status = status,
            Ward = new Ward { Code = "W1", IsClosed = wardClosed }
        };
    }

    // ── IsAssignable ───────────────────────────────────────────────────────────

    [Fact]
    public void IsAssignable_AvailableAndOpenWard_ReturnsTrue()
    {
        var bed = CreateBed(BedStatus.Available, wardClosed: false);
        Assert.True(bed.IsAssignable);
    }

    [Fact]
    public void IsAssignable_OccupiedBed_ReturnsFalse()
    {
        var bed = CreateBed(BedStatus.Occupied, wardClosed: false);
        Assert.False(bed.IsAssignable);
    }

    [Fact]
    public void IsAssignable_ClosedWard_ReturnsFalse()
    {
        var bed = CreateBed(BedStatus.Available, wardClosed: true);
        Assert.False(bed.IsAssignable);
    }

    [Fact]
    public void IsAssignable_OutOfService_ReturnsFalse()
    {
        var bed = CreateBed(BedStatus.OutOfService, wardClosed: false);
        Assert.False(bed.IsAssignable);
    }

    [Fact]
    public void IsAssignable_NullWard_ReturnsFalse()
    {
        var bed = new Bed { Status = BedStatus.Available, Ward = null! };
        Assert.False(bed.IsAssignable);
    }

    // ── ChangeStatus ───────────────────────────────────────────────────────────

    [Fact]
    public void ChangeStatus_UpdatesStatusAndAddsHistory()
    {
        var bed = CreateBed(BedStatus.Available);
        var at = new DateTime(2024, 5, 1, 10, 0, 0);

        bed.ChangeStatus(BedStatus.Occupied, at, "nurse1", "Patient admitted");

        Assert.Equal(BedStatus.Occupied, bed.Status);
        Assert.Single(bed.StatusChanges);
    }

    [Fact]
    public void ChangeStatus_StatusChange_HasCorrectFromAndTo()
    {
        var bed = CreateBed(BedStatus.Available);
        var at = DateTime.UtcNow;

        bed.ChangeStatus(BedStatus.OutOfService, at, "admin", "Maintenance");

        var change = Assert.Single(bed.StatusChanges);
        Assert.Equal(BedStatus.Available, change.FromStatus);
        Assert.Equal(BedStatus.OutOfService, change.ToStatus);
    }

    [Fact]
    public void ChangeStatus_MultipleChanges_AllRecorded()
    {
        var bed = CreateBed(BedStatus.Available);
        bed.ChangeStatus(BedStatus.Occupied, DateTime.UtcNow, "user1", "r1");
        bed.ChangeStatus(BedStatus.Available, DateTime.UtcNow, "user2", "r2");

        Assert.Equal(2, bed.StatusChanges.Count);
        Assert.Equal(BedStatus.Available, bed.Status);
    }

    [Fact]
    public void Status_DefaultsToAvailable()
    {
        var bed = new Bed();
        Assert.Equal(BedStatus.Available, bed.Status);
    }
}

public class WardTests
{
    [Fact]
    public void Ward_DefaultProperties_AreCorrect()
    {
        var ward = new Ward();
        Assert.Equal(string.Empty, ward.Code);
        Assert.Equal(string.Empty, ward.Name);
        Assert.False(ward.IsClosed);
        Assert.NotNull(ward.Beds);
        Assert.NotNull(ward.Stock);
        Assert.NotNull(ward.Shifts);
    }

    [Fact]
    public void Ward_CanSetAllProperties()
    {
        var ward = new Ward
        {
            Id = 1,
            Code = "ICU",
            Name = "Intensive Care Unit",
            Floor = 3,
            DailyRate = 500m,
            IsClosed = false
        };
        Assert.Equal("ICU", ward.Code);
        Assert.Equal(3, ward.Floor);
        Assert.Equal(500m, ward.DailyRate);
    }
}

public class PractitionerTests
{
    // ── DisplayName ────────────────────────────────────────────────────────────

    [Fact]
    public void DisplayName_Physician_ReturnsDrPrefix()
    {
        var p = new Practitioner
        {
            FirstName = "Alice",
            LastName = "Brown",
            Type = PractitionerType.Physician
        };
        Assert.Equal("Dr. Alice Brown", p.DisplayName);
    }

    [Fact]
    public void DisplayName_Nurse_ReturnsRnSuffix()
    {
        var p = new Practitioner
        {
            FirstName = "Bob",
            LastName = "Green",
            Type = PractitionerType.Nurse
        };
        Assert.Equal("Bob Green, RN", p.DisplayName);
    }

    [Fact]
    public void IsActive_DefaultsToTrue()
    {
        var p = new Practitioner();
        Assert.True(p.IsActive);
    }

    [Fact]
    public void Collections_DefaultToEmptyLists()
    {
        var p = new Practitioner();
        Assert.NotNull(p.StatusChanges);
        Assert.NotNull(p.Shifts);
        Assert.NotNull(p.Appointments);
    }
}

public class InsuranceCoverageTests
{
    private static InsuranceCoverage CreateCoverage(
        DateTime from, DateTime? to = null, bool isActive = true)
    {
        return new InsuranceCoverage
        {
            EffectiveFrom = from,
            EffectiveTo = to,
            IsActive = isActive,
            CoveragePercent = 80m
        };
    }

    // ── CoversDate ─────────────────────────────────────────────────────────────

    [Fact]
    public void CoversDate_DateWithinRange_ReturnsTrue()
    {
        var cov = CreateCoverage(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
        Assert.True(cov.CoversDate(new DateTime(2024, 6, 15)));
    }

    [Fact]
    public void CoversDate_DateBeforeRange_ReturnsFalse()
    {
        var cov = CreateCoverage(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
        Assert.False(cov.CoversDate(new DateTime(2023, 12, 31)));
    }

    [Fact]
    public void CoversDate_DateAfterRange_ReturnsFalse()
    {
        var cov = CreateCoverage(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
        Assert.False(cov.CoversDate(new DateTime(2025, 1, 1)));
    }

    [Fact]
    public void CoversDate_OpenEndedCoverage_ReturnsTrue()
    {
        var cov = CreateCoverage(new DateTime(2024, 1, 1), null);
        Assert.True(cov.CoversDate(new DateTime(2030, 1, 1)));
    }

    [Fact]
    public void CoversDate_InactivePolicy_ReturnsFalse()
    {
        var cov = CreateCoverage(new DateTime(2024, 1, 1), null, isActive: false);
        Assert.False(cov.CoversDate(new DateTime(2024, 6, 15)));
    }

    [Fact]
    public void CoversDate_OnEffectiveFromDate_ReturnsTrue()
    {
        var from = new DateTime(2024, 3, 1);
        var cov = CreateCoverage(from, new DateTime(2024, 12, 31));
        Assert.True(cov.CoversDate(from));
    }

    [Fact]
    public void CoversDate_OnEffectiveToDate_ReturnsTrue()
    {
        var to = new DateTime(2024, 12, 31);
        var cov = CreateCoverage(new DateTime(2024, 1, 1), to);
        Assert.True(cov.CoversDate(to));
    }

    // ── Overlaps ───────────────────────────────────────────────────────────────

    [Fact]
    public void Overlaps_OverlappingRange_ReturnsTrue()
    {
        var cov = CreateCoverage(new DateTime(2024, 1, 1), new DateTime(2024, 6, 30));
        Assert.True(cov.Overlaps(new DateTime(2024, 5, 1), new DateTime(2024, 12, 31)));
    }

    [Fact]
    public void Overlaps_NonOverlappingRange_ReturnsFalse()
    {
        var cov = CreateCoverage(new DateTime(2024, 1, 1), new DateTime(2024, 3, 31));
        Assert.False(cov.Overlaps(new DateTime(2024, 4, 1), new DateTime(2024, 12, 31)));
    }

    [Fact]
    public void Overlaps_OpenEndedBothSides_ReturnsTrue()
    {
        var cov = CreateCoverage(new DateTime(2024, 1, 1), null);
        Assert.True(cov.Overlaps(new DateTime(2025, 1, 1), null));
    }

    [Fact]
    public void Overlaps_AdjacentRanges_ReturnsFalse()
    {
        var cov = CreateCoverage(new DateTime(2024, 1, 1), new DateTime(2024, 3, 31));
        Assert.False(cov.Overlaps(new DateTime(2024, 4, 1), new DateTime(2024, 12, 31)));
    }
}

public class ClinicalOrderTests
{
    private static ClinicalOrder CreateOrder(OrderStatus status = OrderStatus.Placed)
    {
        return new ClinicalOrder
        {
            Id = 1,
            OrderNumber = "ORD-001",
            Type = OrderType.Lab,
            Status = status,
            PlacedAt = new DateTime(2024, 5, 1),
            PlacedBy = "dr.smith"
        };
    }

    // ── Description ───────────────────────────────────────────────────────────

    [Fact]
    public void Description_LabOrderWithNoDetail_ReturnsLabTest()
    {
        var order = CreateOrder();
        order.LabDetail = null;
        Assert.Equal("Lab test", order.Description);
    }

    [Fact]
    public void Description_MedicationOrderWithNoDetail_ReturnsMedication()
    {
        var order = new ClinicalOrder { Type = OrderType.Medication };
        Assert.Equal("Medication", order.Description);
    }

    [Fact]
    public void Description_LabOrderWithDefinition_ReturnsTestName()
    {
        var order = CreateOrder();
        order.LabDetail = new LabOrderDetail
        {
            TestDefinition = new LabTestDefinition { Name = "CBC" }
        };
        Assert.Equal("CBC", order.Description);
    }

    // ── ChangeStatus ───────────────────────────────────────────────────────────

    [Fact]
    public void ChangeStatus_ToInProgress_SetsStartedAt()
    {
        var order = CreateOrder(OrderStatus.Placed);
        var at = new DateTime(2024, 5, 2, 9, 0, 0);

        order.ChangeStatus(OrderStatus.InProgress, at, "lab_tech", null);

        Assert.Equal(OrderStatus.InProgress, order.Status);
        Assert.Equal(at, order.StartedAt);
    }

    [Fact]
    public void ChangeStatus_ToCompleted_SetsCompletedAt()
    {
        var order = CreateOrder(OrderStatus.InProgress);
        order.StartedAt = new DateTime(2024, 5, 2);
        var at = new DateTime(2024, 5, 2, 12, 0, 0);

        order.ChangeStatus(OrderStatus.Completed, at, "lab_tech", null);

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(at, order.CompletedAt);
    }

    [Fact]
    public void ChangeStatus_ToCompleted_SetsStartedAtIfNotAlreadySet()
    {
        var order = CreateOrder(OrderStatus.Placed);
        var at = new DateTime(2024, 5, 2, 12, 0, 0);

        order.ChangeStatus(OrderStatus.Completed, at, "lab_tech", null);

        Assert.Equal(at, order.StartedAt);
        Assert.Equal(at, order.CompletedAt);
    }

    [Fact]
    public void ChangeStatus_ToCancelled_SetsCancelledAtAndReason()
    {
        var order = CreateOrder(OrderStatus.Placed);
        var at = new DateTime(2024, 5, 2, 8, 0, 0);

        order.ChangeStatus(OrderStatus.Cancelled, at, "admin", "Duplicate order");

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(at, order.CancelledAt);
        Assert.Equal("Duplicate order", order.CancellationReason);
    }

    [Fact]
    public void ChangeStatus_AddsStatusChangeRecord()
    {
        var order = CreateOrder(OrderStatus.Placed);
        order.ChangeStatus(OrderStatus.InProgress, DateTime.UtcNow, "user", null);

        Assert.Single(order.StatusChanges);
    }

    [Fact]
    public void Status_DefaultsToPlaced()
    {
        var order = new ClinicalOrder();
        Assert.Equal(OrderStatus.Placed, order.Status);
    }

    [Fact]
    public void Priority_DefaultsToRoutine()
    {
        var order = new ClinicalOrder();
        Assert.Equal(OrderPriority.Routine, order.Priority);
    }
}

public class InvoiceTests
{
    private static Invoice CreateInvoice(InvoiceStatus status = InvoiceStatus.Draft)
    {
        return new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-001",
            Status = status,
            CreatedAt = new DateTime(2024, 5, 1),
            PatientResponsibility = 1000m
        };
    }

    // ── AmountPaid ─────────────────────────────────────────────────────────────

    [Fact]
    public void AmountPaid_NoPayments_ReturnsZero()
    {
        var invoice = CreateInvoice();
        Assert.Equal(0m, invoice.AmountPaid);
    }

    [Fact]
    public void AmountPaid_WithPayments_ReturnsSumOfAmounts()
    {
        var invoice = CreateInvoice();
        invoice.Payments.Add(new Payment { Amount = 300m });
        invoice.Payments.Add(new Payment { Amount = 200m });
        Assert.Equal(500m, invoice.AmountPaid);
    }

    // ── BalanceDue ─────────────────────────────────────────────────────────────

    [Fact]
    public void BalanceDue_NoPayments_ReturnsFullResponsibility()
    {
        var invoice = CreateInvoice();
        Assert.Equal(1000m, invoice.BalanceDue);
    }

    [Fact]
    public void BalanceDue_PartialPayment_ReturnsRemainder()
    {
        var invoice = CreateInvoice();
        invoice.Payments.Add(new Payment { Amount = 400m });
        Assert.Equal(600m, invoice.BalanceDue);
    }

    [Fact]
    public void BalanceDue_FullPayment_ReturnsZero()
    {
        var invoice = CreateInvoice();
        invoice.Payments.Add(new Payment { Amount = 1000m });
        Assert.Equal(0m, invoice.BalanceDue);
    }

    // ── ChangeStatus ───────────────────────────────────────────────────────────

    [Fact]
    public void ChangeStatus_UpdatesStatusAndAddsHistory()
    {
        var invoice = CreateInvoice(InvoiceStatus.Draft);
        var at = new DateTime(2024, 5, 10);

        invoice.ChangeStatus(InvoiceStatus.Issued, at, "billing_clerk", "Issued to patient");

        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Single(invoice.StatusChanges);
    }

    [Fact]
    public void ChangeStatus_StatusChange_HasCorrectFromAndTo()
    {
        var invoice = CreateInvoice(InvoiceStatus.Draft);
        var at = DateTime.UtcNow;

        invoice.ChangeStatus(InvoiceStatus.Paid, at, "clerk", null);

        var change = Assert.Single(invoice.StatusChanges);
        Assert.Equal(InvoiceStatus.Draft, change.FromStatus);
        Assert.Equal(InvoiceStatus.Paid, change.ToStatus);
    }

    [Fact]
    public void Status_DefaultsToDraft()
    {
        var invoice = new Invoice();
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
    }
}

public class WardStockTests
{
    private static WardStock CreateStock(int onHand = 50, int reorderLevel = 10)
    {
        return new WardStock
        {
            Id = 1,
            QuantityOnHand = onHand,
            ReorderLevel = reorderLevel
        };
    }

    // ── BelowReorderLevel ──────────────────────────────────────────────────────

    [Fact]
    public void BelowReorderLevel_QuantityAboveReorder_ReturnsFalse()
    {
        var stock = CreateStock(onHand: 50, reorderLevel: 10);
        Assert.False(stock.BelowReorderLevel);
    }

    [Fact]
    public void BelowReorderLevel_QuantityAtReorder_ReturnsTrue()
    {
        var stock = CreateStock(onHand: 10, reorderLevel: 10);
        Assert.True(stock.BelowReorderLevel);
    }

    [Fact]
    public void BelowReorderLevel_QuantityBelowReorder_ReturnsTrue()
    {
        var stock = CreateStock(onHand: 5, reorderLevel: 10);
        Assert.True(stock.BelowReorderLevel);
    }

    // ── Apply ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Apply_Restock_IncreasesQuantity()
    {
        var stock = CreateStock(onHand: 20);
        stock.Apply(StockMovementType.Restock, 30, DateTime.UtcNow, "pharmacist", "REF-001");
        Assert.Equal(50, stock.QuantityOnHand);
    }

    [Fact]
    public void Apply_Dispense_DecreasesQuantity()
    {
        var stock = CreateStock(onHand: 50);
        stock.Apply(StockMovementType.Dispense, -5, DateTime.UtcNow, "nurse", "ORD-001");
        Assert.Equal(45, stock.QuantityOnHand);
    }

    [Fact]
    public void Apply_AddsMovementRecord()
    {
        var stock = CreateStock(onHand: 20);
        stock.Apply(StockMovementType.Restock, 10, DateTime.UtcNow, "pharmacist", "REF-001");
        Assert.Single(stock.Movements);
    }

    [Fact]
    public void Apply_MovementRecord_HasCorrectQuantityAfter()
    {
        var stock = CreateStock(onHand: 20);
        stock.Apply(StockMovementType.Restock, 10, DateTime.UtcNow, "pharmacist", "REF-001");
        var movement = Assert.Single(stock.Movements);
        Assert.Equal(30, movement.QuantityAfter);
    }
}

public class AppointmentTests
{
    private static Appointment CreateAppointment(
        DateTime? starts = null, DateTime? ends = null,
        AppointmentStatus status = AppointmentStatus.Booked)
    {
        var s = starts ?? new DateTime(2024, 6, 1, 9, 0, 0);
        var e = ends ?? new DateTime(2024, 6, 1, 9, 30, 0);
        return new Appointment
        {
            Id = 1,
            StartsAt = s,
            EndsAt = e,
            Status = status,
            Reason = "Checkup",
            BookedAt = DateTime.UtcNow,
            BookedBy = "receptionist"
        };
    }

    // ── DurationMinutes ────────────────────────────────────────────────────────

    [Fact]
    public void DurationMinutes_ThirtyMinuteAppointment_Returns30()
    {
        var appt = CreateAppointment(
            new DateTime(2024, 6, 1, 9, 0, 0),
            new DateTime(2024, 6, 1, 9, 30, 0));
        Assert.Equal(30, appt.DurationMinutes);
    }

    [Fact]
    public void DurationMinutes_OneHourAppointment_Returns60()
    {
        var appt = CreateAppointment(
            new DateTime(2024, 6, 1, 9, 0, 0),
            new DateTime(2024, 6, 1, 10, 0, 0));
        Assert.Equal(60, appt.DurationMinutes);
    }

    // ── ChangeStatus ───────────────────────────────────────────────────────────

    [Fact]
    public void ChangeStatus_UpdatesStatusAndAddsHistory()
    {
        var appt = CreateAppointment();
        var at = new DateTime(2024, 6, 1, 9, 30, 0);

        appt.ChangeStatus(AppointmentStatus.Completed, at, "dr.jones", "All good");

        Assert.Equal(AppointmentStatus.Completed, appt.Status);
        Assert.Single(appt.StatusChanges);
    }

    [Fact]
    public void ChangeStatus_StatusChange_HasCorrectFromAndTo()
    {
        var appt = CreateAppointment(status: AppointmentStatus.Booked);
        var at = DateTime.UtcNow;

        appt.ChangeStatus(AppointmentStatus.Cancelled, at, "admin", "No show");

        var change = Assert.Single(appt.StatusChanges);
        Assert.Equal(AppointmentStatus.Booked, change.FromStatus);
        Assert.Equal(AppointmentStatus.Cancelled, change.ToStatus);
    }

    [Fact]
    public void Status_DefaultsToBooked()
    {
        var appt = new Appointment();
        Assert.Equal(AppointmentStatus.Booked, appt.Status);
    }
}

public class ShiftTests
{
    private static Shift CreateShift(DateTime starts, DateTime ends)
    {
        return new Shift
        {
            Id = 1,
            StartsAt = starts,
            EndsAt = ends,
            Status = ShiftStatus.Scheduled
        };
    }

    // ── Overlaps ───────────────────────────────────────────────────────────────

    [Fact]
    public void Overlaps_OverlappingShifts_ReturnsTrue()
    {
        var shift = CreateShift(
            new DateTime(2024, 6, 1, 8, 0, 0),
            new DateTime(2024, 6, 1, 16, 0, 0));

        Assert.True(shift.Overlaps(
            new DateTime(2024, 6, 1, 12, 0, 0),
            new DateTime(2024, 6, 1, 20, 0, 0)));
    }

    [Fact]
    public void Overlaps_NonOverlappingShifts_ReturnsFalse()
    {
        var shift = CreateShift(
            new DateTime(2024, 6, 1, 8, 0, 0),
            new DateTime(2024, 6, 1, 16, 0, 0));

        Assert.False(shift.Overlaps(
            new DateTime(2024, 6, 1, 16, 0, 0),
            new DateTime(2024, 6, 2, 0, 0, 0)));
    }

    [Fact]
    public void Overlaps_ContainedShift_ReturnsTrue()
    {
        var shift = CreateShift(
            new DateTime(2024, 6, 1, 8, 0, 0),
            new DateTime(2024, 6, 1, 16, 0, 0));

        Assert.True(shift.Overlaps(
            new DateTime(2024, 6, 1, 10, 0, 0),
            new DateTime(2024, 6, 1, 14, 0, 0)));
    }

    [Fact]
    public void Overlaps_ShiftBeforeRange_ReturnsFalse()
    {
        var shift = CreateShift(
            new DateTime(2024, 6, 1, 8, 0, 0),
            new DateTime(2024, 6, 1, 12, 0, 0));

        Assert.False(shift.Overlaps(
            new DateTime(2024, 6, 1, 12, 0, 0),
            new DateTime(2024, 6, 1, 20, 0, 0)));
    }

    [Fact]
    public void Status_DefaultsToScheduled()
    {
        var shift = new Shift();
        Assert.Equal(ShiftStatus.Scheduled, shift.Status);
    }
}

public class LabTestDefinitionTests
{
    private static LabTestDefinition CreateNumericTest(
        decimal? low = 4.0m, decimal? high = 10.0m)
    {
        return new LabTestDefinition
        {
            Id = 1,
            Code = "CBC",
            Name = "Complete Blood Count",
            ResultType = ResultValueType.Numeric,
            ReferenceLow = low,
            ReferenceHigh = high,
            Unit = "g/dL"
        };
    }

    private static LabTestDefinition CreateTextTest(string? referenceText = "Negative")
    {
        return new LabTestDefinition
        {
            Id = 2,
            Code = "CULT",
            Name = "Culture",
            ResultType = ResultValueType.Text,
            ReferenceText = referenceText
        };
    }

    // ── ReferenceRangeDisplay ──────────────────────────────────────────────────

    [Fact]
    public void ReferenceRangeDisplay_NumericType_ShowsRange()
    {
        var test = CreateNumericTest(4.0m, 10.0m);
        test.Unit = "g/dL";
        Assert.Contains("4", test.ReferenceRangeDisplay);
        Assert.Contains("10", test.ReferenceRangeDisplay);
    }

    [Fact]
    public void ReferenceRangeDisplay_TextType_ShowsReferenceText()
    {
        var test = CreateTextTest("Negative");
        Assert.Equal("Negative", test.ReferenceRangeDisplay);
    }

    [Fact]
    public void ReferenceRangeDisplay_TextType_NullReferenceText_ReturnsDash()
    {
        var test = CreateTextTest(null);
        Assert.Equal("-", test.ReferenceRangeDisplay);
    }

    // ── Evaluate ───────────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_NumericWithinRange_ReturnsNormal()
    {
        var test = CreateNumericTest(4.0m, 10.0m);
        Assert.Equal(ResultFlag.Normal, test.Evaluate(7.0m, null));
    }

    [Fact]
    public void Evaluate_NumericBelowLow_ReturnsLow()
    {
        var test = CreateNumericTest(4.0m, 10.0m);
        Assert.Equal(ResultFlag.Low, test.Evaluate(2.0m, null));
    }

    [Fact]
    public void Evaluate_NumericAboveHigh_ReturnsHigh()
    {
        var test = CreateNumericTest(4.0m, 10.0m);
        Assert.Equal(ResultFlag.High, test.Evaluate(12.0m, null));
    }

    [Fact]
    public void Evaluate_NumericAtLowBoundary_ReturnsNormal()
    {
        var test = CreateNumericTest(4.0m, 10.0m);
        Assert.Equal(ResultFlag.Normal, test.Evaluate(4.0m, null));
    }

    [Fact]
    public void Evaluate_NumericAtHighBoundary_ReturnsNormal()
    {
        var test = CreateNumericTest(4.0m, 10.0m);
        Assert.Equal(ResultFlag.Normal, test.Evaluate(10.0m, null));
    }

    [Fact]
    public void Evaluate_TextMatchesReference_ReturnsNormal()
    {
        var test = CreateTextTest("Negative");
        Assert.Equal(ResultFlag.Normal, test.Evaluate(null, "Negative"));
    }

    [Fact]
    public void Evaluate_TextMatchesCaseInsensitive_ReturnsNormal()
    {
        var test = CreateTextTest("Negative");
        Assert.Equal(ResultFlag.Normal, test.Evaluate(null, "NEGATIVE"));
    }

    [Fact]
    public void Evaluate_TextDoesNotMatch_ReturnsAbnormal()
    {
        var test = CreateTextTest("Negative");
        Assert.Equal(ResultFlag.Abnormal, test.Evaluate(null, "Positive"));
    }

    [Fact]
    public void Evaluate_NumericNullValue_ReturnsNormal()
    {
        var test = CreateNumericTest(4.0m, 10.0m);
        Assert.Equal(ResultFlag.Normal, test.Evaluate(null, null));
    }

    [Fact]
    public void Evaluate_NoReferenceLow_OnlyChecksHigh()
    {
        var test = CreateNumericTest(null, 10.0m);
        Assert.Equal(ResultFlag.Normal, test.Evaluate(1.0m, null));
        Assert.Equal(ResultFlag.High, test.Evaluate(11.0m, null));
    }

    [Fact]
    public void Evaluate_NoReferenceHigh_OnlyChecksLow()
    {
        var test = CreateNumericTest(4.0m, null);
        Assert.Equal(ResultFlag.Normal, test.Evaluate(100.0m, null));
        Assert.Equal(ResultFlag.Low, test.Evaluate(1.0m, null));
    }
}

public class DrugTests
{
    [Fact]
    public void DisplayName_CombinesNameStrengthForm()
    {
        var drug = new Drug
        {
            Name = "Aspirin",
            Strength = "500mg",
            Form = "Tablet"
        };
        Assert.Equal("Aspirin 500mg Tablet", drug.DisplayName);
    }

    [Fact]
    public void IsActive_DefaultsToTrue()
    {
        var drug = new Drug();
        Assert.True(drug.IsActive);
    }

    [Fact]
    public void Stock_DefaultsToEmptyList()
    {
        var drug = new Drug();
        Assert.NotNull(drug.Stock);
        Assert.Empty(drug.Stock);
    }
}

public class EntityTests
{
    private class ConcreteEntity : CarePath.Models.Entities.Entity { }

    [Fact]
    public void Entity_Id_DefaultsToZero()
    {
        var entity = new ConcreteEntity();
        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public void Entity_Id_CanBeSet()
    {
        var entity = new ConcreteEntity { Id = 42 };
        Assert.Equal(42, entity.Id);
    }
}
