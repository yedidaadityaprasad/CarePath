using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IWardService
{
    Task<IReadOnlyList<WardSummary>> ListWardsAsync(bool includeClosed);
    Task<WardDetailsViewModel> GetWardDetailsAsync(int id);
    Task<WardFormViewModel> GetWardForEditAsync(int id);
    Task<int> CreateWardAsync(WardFormViewModel model);
    Task UpdateWardAsync(WardFormViewModel model);
    Task CloseWardAsync(int id, string reason);
    Task ReopenWardAsync(int id);

    Task<IReadOnlyList<Bed>> ListBedsAsync(int? wardId, BedStatus? status);
    Task<BedDetailsViewModel> GetBedDetailsAsync(int id);
    Task<BedFormViewModel> GetBedForEditAsync(int id);
    Task<int> AddBedAsync(BedFormViewModel model);
    Task UpdateBedAsync(BedFormViewModel model);
    Task SetBedOutOfServiceAsync(int id, string reason);
    Task ReturnBedToServiceAsync(int id, string reason);
}
