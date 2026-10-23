using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IPractitionerService
{
    Task<IReadOnlyList<Practitioner>> SearchAsync(string? search, PractitionerType? type, bool activeOnly);
    Task<PractitionerDetailsViewModel> GetDetailsAsync(int id);
    Task<PractitionerFormViewModel> GetForEditAsync(int id);
    Task<int> CreateAsync(PractitionerFormViewModel model);
    Task UpdateAsync(PractitionerFormViewModel model);
    Task DeactivateAsync(int id, string reason);
    Task ActivateAsync(int id, string reason);
}
