using System;
using System.Collections.Generic;
using Xunit;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Tests.Infrastructure;

public class StatusBadgesTests
{
    // ── BedStatus ──────────────────────────────────────────────────────────────

    [Fact]
    public void For_BedStatus_Available_ReturnsBgSuccess()
    {
        Assert.Equal("bg-success", StatusBadges.For(BedStatus.Available));
    }

    [Fact]
    public void For_BedStatus_Occupied_ReturnsBgPrimary()
    {
        Assert.Equal("bg-primary", StatusBadges.For(BedStatus.Occupied));
    }

    [Fact]
    public void For_BedStatus_OutOfService_ReturnsBgSecondary()
    {
        Assert.Equal("bg-secondary", StatusBadges.For(BedStatus.OutOfService));
    }

    // ── AdmissionStatus ────────────────────────────────────────────────────────

    [Fact]
    public void For_AdmissionStatus_Open_ReturnsBgPrimary()
    {
        Assert.Equal("bg-primary", StatusBadges.For(AdmissionStatus.Open));
    }

    [Fact]
    public void For_AdmissionStatus_Discharged_ReturnsBgSecondary()
    {
        Assert.Equal("bg-secondary", StatusBadges.For(AdmissionStatus.Discharged));
    }

    // ── ShiftStatus ────────────────────────────────────────────────────────────

    [Fact]
    public void For_ShiftStatus_Scheduled_ReturnsBgInfoTextDark()
    {
        Assert.Equal("bg-info text-dark", StatusBadges.For(ShiftStatus.Scheduled));
    }

    [Fact]
    public void For_ShiftStatus_Cancelled_ReturnsBgSecondary()
    {
        Assert.Equal("bg-secondary", StatusBadges.For(ShiftStatus.Cancelled));
    }

    // ── AppointmentStatus ──────────────────────────────────────────────────────

    [Fact]
    public void For_AppointmentStatus_Booked_ReturnsBgInfoTextDark()
    {
        Assert.Equal("bg-info text-dark", StatusBadges.For(AppointmentStatus.Booked));
    }

    [Fact]
    public void For_AppointmentStatus_Completed_ReturnsBgSuccess()
    {
        Assert.Equal("bg-success", StatusBadges.For(AppointmentStatus.Completed));
    }

    [Fact]
    public void For_AppointmentStatus_Cancelled_ReturnsBgSecondary()
    {
        Assert.Equal("bg-secondary", StatusBadges.For(AppointmentStatus.Cancelled));
    }

    // ── OrderStatus ────────────────────────────────────────────────────────────

    [Fact]
    public void For_OrderStatus_Placed_ReturnsBgWarningTextDark()
    {
        Assert.Equal("bg-warning text-dark", StatusBadges.For(OrderStatus.Placed));
    }

    [Fact]
    public void For_OrderStatus_InProgress_ReturnsBgInfoTextDark()
    {
        Assert.Equal("bg-info text-dark", StatusBadges.For(OrderStatus.InProgress));
    }

    [Fact]
    public void For_OrderStatus_Completed_ReturnsBgSuccess()
    {
        Assert.Equal("bg-success", StatusBadges.For(OrderStatus.Completed));
    }

    [Fact]
    public void For_OrderStatus_Cancelled_ReturnsBgSecondary()
    {
        Assert.Equal("bg-secondary", StatusBadges.For(OrderStatus.Cancelled));
    }

    // ── OrderPriority ──────────────────────────────────────────────────────────

    [Fact]
    public void For_OrderPriority_Stat_ReturnsBgDanger()
    {
        Assert.Equal("bg-danger", StatusBadges.For(OrderPriority.Stat));
    }

    [Fact]
    public void For_OrderPriority_Urgent_ReturnsBgWarningTextDark()
    {
        Assert.Equal("bg-warning text-dark", StatusBadges.For(OrderPriority.Urgent));
    }

    [Fact]
    public void For_OrderPriority_Routine_ReturnsBgLightTextDarkBorder()
    {
        Assert.Equal("bg-light text-dark border", StatusBadges.For(OrderPriority.Routine));
    }

    // ── InvoiceStatus ──────────────────────────────────────────────────────────

    [Fact]
    public void For_InvoiceStatus_Draft_ReturnsBgWarningTextDark()
    {
        Assert.Equal("bg-warning text-dark", StatusBadges.For(InvoiceStatus.Draft));
    }

    [Fact]
    public void For_InvoiceStatus_Issued_ReturnsBgInfoTextDark()
    {
        Assert.Equal("bg-info text-dark", StatusBadges.For(InvoiceStatus.Issued));
    }

    [Fact]
    public void For_InvoiceStatus_Paid_ReturnsBgSuccess()
    {
        Assert.Equal("bg-success", StatusBadges.For(InvoiceStatus.Paid));
    }

    [Fact]
    public void For_InvoiceStatus_Void_ReturnsBgSecondary()
    {
        Assert.Equal("bg-secondary", StatusBadges.For(InvoiceStatus.Void));
    }

    // ── ResultFlag ─────────────────────────────────────────────────────────────

    [Fact]
    public void For_ResultFlag_Normal_ReturnsBgSuccess()
    {
        Assert.Equal("bg-success", StatusBadges.For(ResultFlag.Normal));
    }

    [Fact]
    public void For_ResultFlag_Low_ReturnsBgWarningTextDark()
    {
        Assert.Equal("bg-warning text-dark", StatusBadges.For(ResultFlag.Low));
    }

    [Fact]
    public void For_ResultFlag_High_ReturnsBgWarningTextDark()
    {
        Assert.Equal("bg-warning text-dark", StatusBadges.For(ResultFlag.High));
    }

    [Fact]
    public void For_ResultFlag_Abnormal_ReturnsBgDanger()
    {
        Assert.Equal("bg-danger", StatusBadges.For(ResultFlag.Abnormal));
    }

    // ── Active ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Active_True_ReturnsBgSuccess()
    {
        Assert.Equal("bg-success", StatusBadges.Active(true));
    }

    [Fact]
    public void Active_False_ReturnsBgSecondary()
    {
        Assert.Equal("bg-secondary", StatusBadges.Active(false));
    }
}

public class RolesTests
{
    [Fact]
    public void Admin_ConstantValue_IsAdmin()
    {
        Assert.Equal("Admin", Roles.Admin);
    }

    [Fact]
    public void Physician_ConstantValue_IsPhysician()
    {
        Assert.Equal("Physician", Roles.Physician);
    }

    [Fact]
    public void Nurse_ConstantValue_IsNurse()
    {
        Assert.Equal("Nurse", Roles.Nurse);
    }

    [Fact]
    public void BillingClerk_ConstantValue_IsBillingClerk()
    {
        Assert.Equal("BillingClerk", Roles.BillingClerk);
    }

    [Fact]
    public void Pharmacist_ConstantValue_IsPharmacist()
    {
        Assert.Equal("Pharmacist", Roles.Pharmacist);
    }

    [Fact]
    public void Clinical_ContainsPhysicianAndNurse()
    {
        Assert.Contains("Physician", Roles.Clinical);
        Assert.Contains("Nurse", Roles.Clinical);
    }

    [Fact]
    public void ClinicalAndAdmin_ContainsAdminPhysicianNurse()
    {
        Assert.Contains("Admin", Roles.ClinicalAndAdmin);
        Assert.Contains("Physician", Roles.ClinicalAndAdmin);
        Assert.Contains("Nurse", Roles.ClinicalAndAdmin);
    }

    [Fact]
    public void All_ContainsFiveRoles()
    {
        Assert.Equal(5, Roles.All.Count);
    }

    [Fact]
    public void All_ContainsAllExpectedRoles()
    {
        Assert.Contains("Admin", Roles.All);
        Assert.Contains("Physician", Roles.All);
        Assert.Contains("Nurse", Roles.All);
        Assert.Contains("BillingClerk", Roles.All);
        Assert.Contains("Pharmacist", Roles.All);
    }

    [Fact]
    public void PharmacyAndAdmin_ContainsAdminAndPharmacist()
    {
        Assert.Contains("Admin", Roles.PharmacyAndAdmin);
        Assert.Contains("Pharmacist", Roles.PharmacyAndAdmin);
    }

    [Fact]
    public void BillingViewers_ContainsAdminAndBillingClerk()
    {
        Assert.Contains("Admin", Roles.BillingViewers);
        Assert.Contains("BillingClerk", Roles.BillingViewers);
    }
}

public class ReferenceNumbersTests
{
    [Fact]
    public void Next_ReturnsStringWithPrefix()
    {
        var result = ReferenceNumbers.Next("ADM", new DateTime(2024, 5, 15));
        Assert.StartsWith("ADM-", result);
    }

    [Fact]
    public void Next_ReturnsStringWithDatePart()
    {
        var at = new DateTime(2024, 5, 15);
        var result = ReferenceNumbers.Next("ADM", at);
        Assert.Contains("240515", result);
    }

    [Fact]
    public void Next_ReturnsDifferentValuesOnEachCall()
    {
        var at = new DateTime(2024, 5, 15);
        var r1 = ReferenceNumbers.Next("ADM", at);
        var r2 = ReferenceNumbers.Next("ADM", at);
        // Extremely unlikely to be equal due to random suffix
        Assert.NotEqual(r1, r2);
    }

    [Fact]
    public void Next_HasCorrectFormat_PrefixDashDateDashSuffix()
    {
        var at = new DateTime(2024, 5, 15);
        var result = ReferenceNumbers.Next("LAB", at);
        var parts = result.Split('-');
        Assert.Equal(3, parts.Length);
        Assert.Equal("LAB", parts[0]);
        Assert.Equal("240515", parts[1]);
        Assert.Equal(6, parts[2].Length);
    }

    [Fact]
    public void Next_SuffixContainsOnlyAlphabetChars()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var at = new DateTime(2024, 5, 15);
        for (int i = 0; i < 20; i++)
        {
            var result = ReferenceNumbers.Next("X", at);
            var suffix = result.Split('-')[2];
            foreach (var c in suffix)
            {
                Assert.Contains(c, alphabet);
            }
        }
    }

    [Theory]
    [InlineData("ADM")]
    [InlineData("LAB")]
    [InlineData("INV")]
    [InlineData("ORD")]
    public void Next_DifferentPrefixes_AllWork(string prefix)
    {
        var result = ReferenceNumbers.Next(prefix, DateTime.UtcNow);
        Assert.StartsWith(prefix + "-", result);
    }
}

public class TempDataExtensionsTests
{
    [Fact]
    public void SuccessKey_IsCorrectValue()
    {
        Assert.Equal("Flash.Success", TempDataExtensions.SuccessKey);
    }

    [Fact]
    public void ErrorKey_IsCorrectValue()
    {
        Assert.Equal("Flash.Error", TempDataExtensions.ErrorKey);
    }
}
