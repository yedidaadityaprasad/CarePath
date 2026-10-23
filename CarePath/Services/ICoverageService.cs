using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface ICoverageService
{
    Task<PagedResult<InsuranceCoverage>> SearchAsync(string? search, bool activeOnly, int page, int pageSize = 25);
    Task<CoverageDetailsViewModel> GetDetailsAsync(int id);
    Task<CoverageFormViewModel> GetForEditAsync(int id);
    Task<int> AttachAsync(CoverageFormViewModel model);
    Task UpdateAsync(CoverageFormViewModel model);
    Task TerminateAsync(int id, string reason);

    /// <summary>Returns the best active coverage (highest percentage) for a patient on a date, or null.</summary>
    Task<InsuranceCoverage?> FindActiveCoverageAsync(int patientId, DateTime onDate);
}
