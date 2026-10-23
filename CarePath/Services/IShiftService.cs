using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IShiftService
{
    Task<IReadOnlyList<Shift>> SearchAsync(int? wardId, int? practitionerId, DateTime? from, DateTime? to, bool includeCancelled);
    Task<ShiftDetailsViewModel> GetDetailsAsync(int id);
    Task<ShiftFormViewModel> GetForEditAsync(int id);
    Task<int> ScheduleAsync(ShiftFormViewModel model);
    Task UpdateAsync(ShiftFormViewModel model);
    Task CancelAsync(int id, string reason);
}
