using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IPayerService
{
    Task<IReadOnlyList<Payer>> ListAsync(bool includeInactive);
    Task<PayerDetailsViewModel> GetDetailsAsync(int id);
    Task<PayerFormViewModel> GetForEditAsync(int id);
    Task<int> CreateAsync(PayerFormViewModel model);
    Task UpdateAsync(PayerFormViewModel model);
    Task SetActiveAsync(int id, bool active);
}
