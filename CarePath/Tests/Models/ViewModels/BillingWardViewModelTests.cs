using System;
using System.Collections.Generic;
using Xunit;
using CarePath.Models.ViewModels;
using CarePath.Models.Entities;

namespace CarePath.Tests.Models.ViewModels;

public class BillingViewModelTests
{
    // ── InvoiceIndexViewModel ──────────────────────────────────────────────────

    [Fact]
    public void InvoiceIndexViewModel_DefaultPage_IsOne()
    {
        var vm = new InvoiceIndexViewModel();
        Assert.Equal(1, vm.Page);
    }

    [Fact]
    public void InvoiceIndexViewModel_DefaultStatus_IsNull()
    {
        var vm = new InvoiceIndexViewModel();
        Assert.Null(vm.Status);
    }

    [Fact]
    public void InvoiceIndexViewModel_DefaultSearch_IsNull()
    {
        var vm = new InvoiceIndexViewModel();
        Assert.Null(vm.Search);
    }

    [Fact]
    public void InvoiceIndexViewModel_Results_DefaultsToEmpty()
    {
        var vm = new InvoiceIndexViewModel();
        Assert.NotNull(vm.Results);
        Assert.Empty(vm.Results.Items);
    }

    // ── IssueInvoiceViewModel ──────────────────────────────────────────────────

    [Fact]
    public void IssueInvoiceViewModel_DefaultDueInDays_Is30()
    {
        var vm = new IssueInvoiceViewModel();
        Assert.Equal(30, vm.DueInDays);
    }

    [Fact]
    public void IssueInvoiceViewModel_DefaultInvoiceId_IsZero()
    {
        var vm = new IssueInvoiceViewModel();
        Assert.Equal(0, vm.InvoiceId);
    }

    // ── PaymentViewModel ───────────────────────────────────────────────────────

    [Fact]
    public void PaymentViewModel_DefaultProperties_AreCorrect()
    {
        var vm = new PaymentViewModel();
        Assert.Equal(0, vm.InvoiceId);
        Assert.Equal(string.Empty, vm.InvoiceNumber);
        Assert.Equal(string.Empty, vm.PatientName);
        Assert.Equal(0m, vm.BalanceDue);
        Assert.Null(vm.Amount);
        Assert.Null(vm.Method);
        Assert.Null(vm.Reference);
    }

    // ── InvoiceDetailsViewModel ────────────────────────────────────────────────

    [Fact]
    public void InvoiceDetailsViewModel_Issue_DefaultsToNewInstance()
    {
        var vm = new InvoiceDetailsViewModel { Invoice = new Invoice() };
        Assert.NotNull(vm.Issue);
    }
}

public class WardViewModelTests
{
    // ── WardSummary ────────────────────────────────────────────────────────────

    [Fact]
    public void WardSummary_OccupancyPercent_ZeroTotalBeds_ReturnsZero()
    {
        var summary = new WardSummary { Ward = new Ward(), TotalBeds = 0, Occupied = 0 };
        Assert.Equal(0m, summary.OccupancyPercent);
    }

    [Fact]
    public void WardSummary_OccupancyPercent_HalfOccupied_Returns50()
    {
        var summary = new WardSummary { Ward = new Ward(), TotalBeds = 10, Occupied = 5 };
        Assert.Equal(50.0m, summary.OccupancyPercent);
    }

    [Fact]
    public void WardSummary_OccupancyPercent_FullyOccupied_Returns100()
    {
        var summary = new WardSummary { Ward = new Ward(), TotalBeds = 10, Occupied = 10 };
        Assert.Equal(100.0m, summary.OccupancyPercent);
    }

    [Fact]
    public void WardSummary_OccupancyPercent_RoundsToOneDecimal()
    {
        var summary = new WardSummary { Ward = new Ward(), TotalBeds = 3, Occupied = 1 };
        Assert.Equal(33.3m, summary.OccupancyPercent);
    }

    // ── WardIndexViewModel ─────────────────────────────────────────────────────

    [Fact]
    public void WardIndexViewModel_DefaultIncludeClosed_IsTrue()
    {
        var vm = new WardIndexViewModel();
        Assert.True(vm.IncludeClosed);
    }

    [Fact]
    public void WardIndexViewModel_DefaultWards_IsEmpty()
    {
        var vm = new WardIndexViewModel();
        Assert.NotNull(vm.Wards);
        Assert.Empty(vm.Wards);
    }

    // ── WardFormViewModel ──────────────────────────────────────────────────────

    [Fact]
    public void WardFormViewModel_IsEdit_FalseWhenIdIsZero()
    {
        var vm = new WardFormViewModel { Id = 0 };
        Assert.False(vm.IsEdit);
    }

    [Fact]
    public void WardFormViewModel_IsEdit_TrueWhenIdIsPositive()
    {
        var vm = new WardFormViewModel { Id = 3 };
        Assert.True(vm.IsEdit);
    }

    // ── BedFormViewModel ───────────────────────────────────────────────────────

    [Fact]
    public void BedFormViewModel_IsEdit_FalseWhenIdIsZero()
    {
        var vm = new BedFormViewModel { Id = 0 };
        Assert.False(vm.IsEdit);
    }

    [Fact]
    public void BedFormViewModel_IsEdit_TrueWhenIdIsPositive()
    {
        var vm = new BedFormViewModel { Id = 7 };
        Assert.True(vm.IsEdit);
    }

    [Fact]
    public void BedFormViewModel_DefaultProperties_AreCorrect()
    {
        var vm = new BedFormViewModel();
        Assert.Equal(0, vm.Id);
        Assert.Null(vm.WardId);
        Assert.Null(vm.BedNumber);
        Assert.Null(vm.Notes);
    }

    // ── BedIndexViewModel ──────────────────────────────────────────────────────

    [Fact]
    public void BedIndexViewModel_DefaultProperties_AreCorrect()
    {
        var vm = new BedIndexViewModel();
        Assert.Null(vm.WardId);
        Assert.Null(vm.Status);
        Assert.NotNull(vm.Beds);
        Assert.Empty(vm.Beds);
    }

    // ── WardDetailsViewModel ───────────────────────────────────────────────────

    [Fact]
    public void WardDetailsViewModel_DefaultCollections_AreEmpty()
    {
        var vm = new WardDetailsViewModel { Ward = new Ward() };
        Assert.NotNull(vm.Beds);
        Assert.Empty(vm.Beds);
        Assert.NotNull(vm.OccupantsByBed);
        Assert.Empty(vm.OccupantsByBed);
    }

    // ── BedDetailsViewModel ────────────────────────────────────────────────────

    [Fact]
    public void BedDetailsViewModel_DefaultProperties_AreCorrect()
    {
        var vm = new BedDetailsViewModel { Bed = new Bed() };
        Assert.Null(vm.CurrentAdmission);
        Assert.NotNull(vm.History);
        Assert.Empty(vm.History);
    }
}
