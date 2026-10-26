using System;
using System.Collections.Generic;
using Xunit;
using CarePath.Models.ViewModels;
using CarePath.Models.Entities;

namespace CarePath.Tests.Models.ViewModels;

public class PagedResultTests
{
    // ── Constructor ────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var items = new List<string> { "a", "b", "c" };
        var result = new PagedResult<string>(items, 2, 10, 25);

        Assert.Equal(items, result.Items);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
    }

    // ── TotalPages ─────────────────────────────────────────────────────────────

    [Fact]
    public void TotalPages_ExactDivision_ReturnsCorrectCount()
    {
        var result = new PagedResult<int>(Array.Empty<int>(), 1, 10, 30);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WithRemainder_RoundsUp()
    {
        var result = new PagedResult<int>(Array.Empty<int>(), 1, 10, 25);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void TotalPages_ZeroItems_ReturnsOne()
    {
        var result = new PagedResult<int>(Array.Empty<int>(), 1, 10, 0);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public void TotalPages_SingleItem_ReturnsOne()
    {
        var result = new PagedResult<int>(Array.Empty<int>(), 1, 10, 1);
        Assert.Equal(1, result.TotalPages);
    }

    // ── HasPrevious ────────────────────────────────────────────────────────────

    [Fact]
    public void HasPrevious_FirstPage_ReturnsFalse()
    {
        var result = new PagedResult<int>(Array.Empty<int>(), 1, 10, 50);
        Assert.False(result.HasPrevious);
    }

    [Fact]
    public void HasPrevious_SecondPage_ReturnsTrue()
    {
        var result = new PagedResult<int>(Array.Empty<int>(), 2, 10, 50);
        Assert.True(result.HasPrevious);
    }

    // ── HasNext ────────────────────────────────────────────────────────────────

    [Fact]
    public void HasNext_LastPage_ReturnsFalse()
    {
        var result = new PagedResult<int>(Array.Empty<int>(), 5, 10, 50);
        Assert.False(result.HasNext);
    }

    [Fact]
    public void HasNext_NotLastPage_ReturnsTrue()
    {
        var result = new PagedResult<int>(Array.Empty<int>(), 1, 10, 50);
        Assert.True(result.HasNext);
    }

    // ── Empty ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Empty_DefaultPageSize_Returns25()
    {
        var result = PagedResult<int>.Empty();
        Assert.Equal(25, result.PageSize);
    }

    [Fact]
    public void Empty_CustomPageSize_IsSet()
    {
        var result = PagedResult<int>.Empty(50);
        Assert.Equal(50, result.PageSize);
    }

    [Fact]
    public void Empty_TotalCount_IsZero()
    {
        var result = PagedResult<int>.Empty();
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public void Empty_Page_IsOne()
    {
        var result = PagedResult<int>.Empty();
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public void Empty_Items_IsEmpty()
    {
        var result = PagedResult<int>.Empty();
        Assert.Empty(result.Items);
    }
}

public class ErrorViewModelTests
{
    [Fact]
    public void ShowRequestId_WithRequestId_ReturnsTrue()
    {
        var model = new ErrorViewModel { RequestId = "trace-123" };
        Assert.True(model.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_NullRequestId_ReturnsFalse()
    {
        var model = new ErrorViewModel { RequestId = null };
        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_EmptyRequestId_ReturnsFalse()
    {
        var model = new ErrorViewModel { RequestId = "" };
        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void DefaultTitle_IsSomethingWentWrong()
    {
        var model = new ErrorViewModel();
        Assert.Equal("Something went wrong", model.Title);
    }

    [Fact]
    public void DefaultStatusCode_Is500()
    {
        var model = new ErrorViewModel();
        Assert.Equal(500, model.StatusCode);
    }

    [Fact]
    public void DefaultMessage_IsNotEmpty()
    {
        var model = new ErrorViewModel();
        Assert.NotEmpty(model.Message);
    }
}

public class DashboardViewModelTests
{
    [Fact]
    public void OccupancyPercent_ZeroTotalBeds_ReturnsZero()
    {
        var vm = new DashboardViewModel { TotalBeds = 0, OccupiedBeds = 0 };
        Assert.Equal(0m, vm.OccupancyPercent);
    }

    [Fact]
    public void OccupancyPercent_HalfOccupied_Returns50()
    {
        var vm = new DashboardViewModel { TotalBeds = 100, OccupiedBeds = 50 };
        Assert.Equal(50.0m, vm.OccupancyPercent);
    }

    [Fact]
    public void OccupancyPercent_FullyOccupied_Returns100()
    {
        var vm = new DashboardViewModel { TotalBeds = 50, OccupiedBeds = 50 };
        Assert.Equal(100.0m, vm.OccupancyPercent);
    }

    [Fact]
    public void OccupancyPercent_RoundsToOneDecimalPlace()
    {
        var vm = new DashboardViewModel { TotalBeds = 3, OccupiedBeds = 1 };
        // 1/3 * 100 = 33.333... → rounds to 33.3
        Assert.Equal(33.3m, vm.OccupancyPercent);
    }

    [Fact]
    public void RecentAdmissions_DefaultsToEmptyArray()
    {
        var vm = new DashboardViewModel();
        Assert.NotNull(vm.RecentAdmissions);
        Assert.Empty(vm.RecentAdmissions);
    }

    [Fact]
    public void AllCountProperties_DefaultToZero()
    {
        var vm = new DashboardViewModel();
        Assert.Equal(0, vm.OccupiedBeds);
        Assert.Equal(0, vm.AvailableBeds);
        Assert.Equal(0, vm.TotalBeds);
        Assert.Equal(0, vm.OpenAdmissions);
        Assert.Equal(0, vm.PendingLabOrders);
        Assert.Equal(0, vm.PendingMedicationOrders);
        Assert.Equal(0, vm.DraftInvoices);
        Assert.Equal(0, vm.TodaysAppointments);
        Assert.Equal(0, vm.LowStockItems);
    }
}

public class StatusActionFormModelTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var model = new StatusActionFormModel();
        Assert.Equal(string.Empty, model.Action);
        Assert.Equal(string.Empty, model.Controller);
        Assert.Equal(0, model.Id);
        Assert.Equal(string.Empty, model.ButtonText);
        Assert.Equal("btn-outline-secondary", model.ButtonClass);
        Assert.False(model.RequireReason);
        Assert.Null(model.ConfirmMessage);
        Assert.NotNull(model.Input);
    }
}

public class ReasonActionViewModelTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var model = new ReasonActionViewModel();
        Assert.Equal(0, model.Id);
        Assert.Null(model.Reason);
    }

    [Fact]
    public void CanSetIdAndReason()
    {
        var model = new ReasonActionViewModel { Id = 5, Reason = "Test reason" };
        Assert.Equal(5, model.Id);
        Assert.Equal("Test reason", model.Reason);
    }
}
