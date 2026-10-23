using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IUserAdminService
{
    Task<IReadOnlyList<UserSummary>> ListAsync();
    Task<UserDetailsViewModel> GetDetailsAsync(string id);
    Task<UserEditViewModel> GetForEditAsync(string id);
    Task<string> CreateAsync(UserCreateViewModel model);
    Task UpdateAsync(UserEditViewModel model, string actingUserName);
}
