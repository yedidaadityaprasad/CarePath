using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetAsync();
}
