using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface ILabTestCatalogService
{
    Task<IReadOnlyList<LabTestDefinition>> ListAsync(bool includeInactive);
    Task<LabTestDetailsViewModel> GetDetailsAsync(int id);
    Task<LabTestFormViewModel> GetForEditAsync(int id);
    Task<int> CreateAsync(LabTestFormViewModel model);
    Task UpdateAsync(LabTestFormViewModel model);
    Task SetActiveAsync(int id, bool active);
}
