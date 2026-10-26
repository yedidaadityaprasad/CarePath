using CarePath.Models.ViewModels;

namespace CarePath.Services;

/// <summary>
/// Contract for the home dashboard aggregation service.
/// .NET 10: CancellationToken added for proper request cancellation support.
/// </summary>
public interface IDashboardService
{
    Task<DashboardViewModel> GetAsync(CancellationToken cancellationToken = default);
}
