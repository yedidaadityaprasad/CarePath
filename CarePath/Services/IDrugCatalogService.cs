using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IDrugCatalogService
{
    Task<IReadOnlyList<Drug>> ListAsync(string? search, bool includeInactive);
    Task<DrugDetailsViewModel> GetDetailsAsync(int id);
    Task<DrugFormViewModel> GetForEditAsync(int id);
    Task<int> CreateAsync(DrugFormViewModel model);
    Task UpdateAsync(DrugFormViewModel model);
    Task SetActiveAsync(int id, bool active);
}
