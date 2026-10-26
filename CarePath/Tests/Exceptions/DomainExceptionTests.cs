using System;
using Xunit;
using CarePath.Exceptions;
using Microsoft.AspNetCore.Http;

namespace CarePath.Tests.Exceptions;

public class DomainExceptionTests
{
    // ── DomainValidationException ──────────────────────────────────────────────

    [Fact]
    public void DomainValidationException_Constructor_SetsMessage()
    {
        // Arrange & Act
        var ex = new DomainValidationException("Invalid input");

        // Assert
        Assert.Equal("Invalid input", ex.Message);
    }

    [Fact]
    public void DomainValidationException_Constructor_SetsPropertyName()
    {
        // Arrange & Act
        var ex = new DomainValidationException("Bad value", "FieldName");

        // Assert
        Assert.Equal("FieldName", ex.PropertyName);
    }

    [Fact]
    public void DomainValidationException_PropertyName_NullByDefault()
    {
        var ex = new DomainValidationException("msg");
        Assert.Null(ex.PropertyName);
    }

    [Fact]
    public void DomainValidationException_Title_ReturnsInvalidRequest()
    {
        var ex = new DomainValidationException("msg");
        Assert.Equal("Invalid request", ex.Title);
    }

    [Fact]
    public void DomainValidationException_StatusCode_Returns400()
    {
        var ex = new DomainValidationException("msg");
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public void DomainValidationException_IsAssignableFrom_DomainException()
    {
        var ex = new DomainValidationException("msg");
        Assert.IsAssignableFrom<DomainException>(ex);
    }

    // ── EntityNotFoundException ────────────────────────────────────────────────

    [Fact]
    public void EntityNotFoundException_Constructor_SetsEntityNameAndKey()
    {
        var ex = new EntityNotFoundException("Patient", 42);
        Assert.Equal("Patient", ex.EntityName);
        Assert.Equal(42, ex.Key);
    }

    [Fact]
    public void EntityNotFoundException_Message_ContainsEntityAndKey()
    {
        var ex = new EntityNotFoundException("Patient", 42);
        Assert.Contains("Patient", ex.Message);
        Assert.Contains("42", ex.Message);
    }

    [Fact]
    public void EntityNotFoundException_Title_ReturnsRecordNotFound()
    {
        var ex = new EntityNotFoundException("Ward", 1);
        Assert.Equal("Record not found", ex.Title);
    }

    [Fact]
    public void EntityNotFoundException_StatusCode_Returns404()
    {
        var ex = new EntityNotFoundException("Ward", 1);
        Assert.Equal(StatusCodes.Status404NotFound, ex.StatusCode);
    }

    [Fact]
    public void EntityNotFoundException_PropertyName_CanBeSet()
    {
        var ex = new EntityNotFoundException("Drug", 5, "DrugId");
        Assert.Equal("DrugId", ex.PropertyName);
    }

    [Fact]
    public void EntityNotFoundException_PropertyName_NullByDefault()
    {
        var ex = new EntityNotFoundException("Drug", 5);
        Assert.Null(ex.PropertyName);
    }

    // ── AdmissionClosedException ───────────────────────────────────────────────

    [Fact]
    public void AdmissionClosedException_Message_ContainsAdmissionNumber()
    {
        var ex = new AdmissionClosedException("ADM-001");
        Assert.Contains("ADM-001", ex.Message);
    }

    [Fact]
    public void AdmissionClosedException_Title_ReturnsAdmissionClosed()
    {
        var ex = new AdmissionClosedException("ADM-001");
        Assert.Equal("Admission closed", ex.Title);
    }

    [Fact]
    public void AdmissionClosedException_DefaultPropertyName_IsAdmissionId()
    {
        var ex = new AdmissionClosedException("ADM-001");
        Assert.Equal("AdmissionId", ex.PropertyName);
    }

    [Fact]
    public void AdmissionClosedException_CustomPropertyName_IsSet()
    {
        var ex = new AdmissionClosedException("ADM-001", "CustomProp");
        Assert.Equal("CustomProp", ex.PropertyName);
    }

    [Fact]
    public void AdmissionClosedException_StatusCode_Returns409()
    {
        var ex = new AdmissionClosedException("ADM-001");
        Assert.Equal(StatusCodes.Status409Conflict, ex.StatusCode);
    }

    // ── PatientInactiveException ───────────────────────────────────────────────

    [Fact]
    public void PatientInactiveException_Message_ContainsMrn()
    {
        var ex = new PatientInactiveException("MRN-123");
        Assert.Contains("MRN-123", ex.Message);
    }

    [Fact]
    public void PatientInactiveException_Title_ReturnsPatientIsInactive()
    {
        var ex = new PatientInactiveException("MRN-123");
        Assert.Equal("Patient is inactive", ex.Title);
    }

    [Fact]
    public void PatientInactiveException_DefaultPropertyName_IsPatientId()
    {
        var ex = new PatientInactiveException("MRN-123");
        Assert.Equal("PatientId", ex.PropertyName);
    }

    // ── PractitionerInactiveException ─────────────────────────────────────────

    [Fact]
    public void PractitionerInactiveException_Message_ContainsPractitionerName()
    {
        var ex = new PractitionerInactiveException("Dr. Smith");
        Assert.Contains("Dr. Smith", ex.Message);
    }

    [Fact]
    public void PractitionerInactiveException_Title_ReturnsPractitionerIsInactive()
    {
        var ex = new PractitionerInactiveException("Dr. Smith");
        Assert.Equal("Practitioner is inactive", ex.Title);
    }

    [Fact]
    public void PractitionerInactiveException_DefaultPropertyName_IsPractitionerId()
    {
        var ex = new PractitionerInactiveException("Dr. Smith");
        Assert.Equal("PractitionerId", ex.PropertyName);
    }

    // ── DuplicateValueException ────────────────────────────────────────────────

    [Fact]
    public void DuplicateValueException_Message_ContainsEntityAndValue()
    {
        var ex = new DuplicateValueException("Patient", "Email", "email address", "test@test.com");
        Assert.Contains("patient", ex.Message);
        Assert.Contains("test@test.com", ex.Message);
    }

    [Fact]
    public void DuplicateValueException_Value_IsSet()
    {
        var ex = new DuplicateValueException("Patient", "Email", "email address", "test@test.com");
        Assert.Equal("test@test.com", ex.Value);
    }

    [Fact]
    public void DuplicateValueException_Title_ReturnsDuplicateValue()
    {
        var ex = new DuplicateValueException("Patient", "Email", "email address", "test@test.com");
        Assert.Equal("Duplicate value", ex.Title);
    }

    [Fact]
    public void DuplicateValueException_PropertyName_IsSet()
    {
        var ex = new DuplicateValueException("Patient", "Email", "email address", "test@test.com");
        Assert.Equal("Email", ex.PropertyName);
    }

    // ── ResultAlreadyRecordedException ────────────────────────────────────────

    [Fact]
    public void ResultAlreadyRecordedException_Message_ContainsOrderNumber()
    {
        var ex = new ResultAlreadyRecordedException("LAB-001");
        Assert.Contains("LAB-001", ex.Message);
    }

    [Fact]
    public void ResultAlreadyRecordedException_Title_ReturnsResultAlreadyRecorded()
    {
        var ex = new ResultAlreadyRecordedException("LAB-001");
        Assert.Equal("Result already recorded", ex.Title);
    }

    // ── CoverageRequiredException ──────────────────────────────────────────────

    [Fact]
    public void CoverageRequiredException_Message_ContainsMrnAndDate()
    {
        var date = new DateTime(2024, 6, 15);
        var ex = new CoverageRequiredException("MRN-999", date);
        Assert.Contains("MRN-999", ex.Message);
        Assert.Contains("2024-06-15", ex.Message);
    }

    [Fact]
    public void CoverageRequiredException_Title_ReturnsActiveCoverageRequired()
    {
        var ex = new CoverageRequiredException("MRN-999", DateTime.Today);
        Assert.Equal("Active coverage required", ex.Title);
    }

    [Fact]
    public void CoverageRequiredException_DefaultPropertyName_IsPatientId()
    {
        var ex = new CoverageRequiredException("MRN-999", DateTime.Today);
        Assert.Equal("PatientId", ex.PropertyName);
    }

    // ── InsufficientStockException ─────────────────────────────────────────────

    [Fact]
    public void InsufficientStockException_Message_ContainsDrugAndWard()
    {
        var ex = new InsufficientStockException("Aspirin", "Ward A", 5, 10);
        Assert.Contains("Aspirin", ex.Message);
        Assert.Contains("Ward A", ex.Message);
    }

    [Fact]
    public void InsufficientStockException_OnHandAndRequested_AreSet()
    {
        var ex = new InsufficientStockException("Aspirin", "Ward A", 5, 10);
        Assert.Equal(5, ex.OnHand);
        Assert.Equal(10, ex.Requested);
    }

    [Fact]
    public void InsufficientStockException_Title_ReturnsInsufficientStock()
    {
        var ex = new InsufficientStockException("Aspirin", "Ward A", 5, 10);
        Assert.Equal("Insufficient stock", ex.Title);
    }

    // ── SchedulingConflictException ────────────────────────────────────────────

    [Fact]
    public void SchedulingConflictException_Message_IsSet()
    {
        var ex = new SchedulingConflictException("Practitioner already has a shift at this time.");
        Assert.Equal("Practitioner already has a shift at this time.", ex.Message);
    }

    [Fact]
    public void SchedulingConflictException_Title_ReturnsSchedulingConflict()
    {
        var ex = new SchedulingConflictException("conflict");
        Assert.Equal("Scheduling conflict", ex.Title);
    }

    [Fact]
    public void SchedulingConflictException_DefaultPropertyName_IsStartsAt()
    {
        var ex = new SchedulingConflictException("conflict");
        Assert.Equal("StartsAt", ex.PropertyName);
    }

    [Fact]
    public void SchedulingConflictException_CustomPropertyName_IsSet()
    {
        var ex = new SchedulingConflictException("conflict", "CustomField");
        Assert.Equal("CustomField", ex.PropertyName);
    }

    // ── InvalidStatusTransitionException ──────────────────────────────────────

    [Fact]
    public void InvalidStatusTransitionException_Message_ContainsEntityAndStatus()
    {
        var ex = new InvalidStatusTransitionException("Order", "Completed", "cancel");
        Assert.Contains("order", ex.Message);
        Assert.Contains("Completed", ex.Message);
        Assert.Contains("cancel", ex.Message);
    }

    [Fact]
    public void InvalidStatusTransitionException_CurrentStatusAndAction_AreSet()
    {
        var ex = new InvalidStatusTransitionException("Order", "Completed", "cancel");
        Assert.Equal("Completed", ex.CurrentStatus);
        Assert.Equal("cancel", ex.AttemptedAction);
    }

    [Fact]
    public void InvalidStatusTransitionException_Title_ReturnsActionNotAllowed()
    {
        var ex = new InvalidStatusTransitionException("Order", "Completed", "cancel");
        Assert.Equal("Action not allowed in the current status", ex.Title);
    }

    // ── WardClosedException ────────────────────────────────────────────────────

    [Fact]
    public void WardClosedException_Message_ContainsWardName()
    {
        var ex = new WardClosedException("Ward B");
        Assert.Contains("Ward B", ex.Message);
    }

    [Fact]
    public void WardClosedException_Title_ReturnsWardClosed()
    {
        var ex = new WardClosedException("Ward B");
        Assert.Equal("Ward closed", ex.Title);
    }

    [Fact]
    public void WardClosedException_PropertyName_NullByDefault()
    {
        var ex = new WardClosedException("Ward B");
        Assert.Null(ex.PropertyName);
    }

    [Fact]
    public void WardClosedException_CustomPropertyName_IsSet()
    {
        var ex = new WardClosedException("Ward B", "WardId");
        Assert.Equal("WardId", ex.PropertyName);
    }

    // ── CoverageConflictException ──────────────────────────────────────────────

    [Fact]
    public void CoverageConflictException_Message_ContainsPayerName()
    {
        var from = new DateTime(2024, 1, 1);
        var to = new DateTime(2024, 12, 31);
        var ex = new CoverageConflictException("BlueCross", from, to);
        Assert.Contains("BlueCross", ex.Message);
    }

    [Fact]
    public void CoverageConflictException_Message_ContainsDates()
    {
        var from = new DateTime(2024, 1, 1);
        var to = new DateTime(2024, 12, 31);
        var ex = new CoverageConflictException("BlueCross", from, to);
        Assert.Contains("2024-01-01", ex.Message);
        Assert.Contains("2024-12-31", ex.Message);
    }

    [Fact]
    public void CoverageConflictException_NullTo_ShowsOpenEnded()
    {
        var from = new DateTime(2024, 1, 1);
        var ex = new CoverageConflictException("BlueCross", from, null);
        Assert.Contains("open-ended", ex.Message);
    }

    [Fact]
    public void CoverageConflictException_Title_ReturnsOverlappingCoverage()
    {
        var ex = new CoverageConflictException("BlueCross", DateTime.Today, null);
        Assert.Equal("Overlapping coverage", ex.Title);
    }

    [Fact]
    public void CoverageConflictException_PropertyName_IsEffectiveFrom()
    {
        var ex = new CoverageConflictException("BlueCross", DateTime.Today, null);
        Assert.Equal("EffectiveFrom", ex.PropertyName);
    }

    // ── BedUnavailableException ────────────────────────────────────────────────

    [Fact]
    public void BedUnavailableException_Message_ContainsBedLabelAndStatus()
    {
        var ex = new BedUnavailableException("A-101", CarePath.Models.Entities.BedStatus.Occupied);
        Assert.Contains("A-101", ex.Message);
        Assert.Contains("Occupied", ex.Message);
    }

    [Fact]
    public void BedUnavailableException_Status_IsSet()
    {
        var ex = new BedUnavailableException("A-101", CarePath.Models.Entities.BedStatus.OutOfService);
        Assert.Equal(CarePath.Models.Entities.BedStatus.OutOfService, ex.Status);
    }

    [Fact]
    public void BedUnavailableException_Title_ReturnsBedUnavailable()
    {
        var ex = new BedUnavailableException("A-101", CarePath.Models.Entities.BedStatus.Occupied);
        Assert.Equal("Bed unavailable", ex.Title);
    }

    [Fact]
    public void BedUnavailableException_DefaultPropertyName_IsBedId()
    {
        var ex = new BedUnavailableException("A-101", CarePath.Models.Entities.BedStatus.Occupied);
        Assert.Equal("BedId", ex.PropertyName);
    }

    [Fact]
    public void BedUnavailableException_CustomPropertyName_IsSet()
    {
        var ex = new BedUnavailableException("A-101", CarePath.Models.Entities.BedStatus.Occupied, "CustomBedProp");
        Assert.Equal("CustomBedProp", ex.PropertyName);
    }
}
