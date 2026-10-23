using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IAppointmentService
{
    Task<IReadOnlyList<Appointment>> SearchAsync(DateTime? from, DateTime? to, int? practitionerId, AppointmentStatus? status);
    Task<AppointmentDetailsViewModel> GetDetailsAsync(int id);
    Task<AppointmentFormViewModel> GetForEditAsync(int id);
    Task<int> BookAsync(AppointmentFormViewModel model);
    Task RescheduleAsync(AppointmentFormViewModel model);
    Task CancelAsync(int id, string reason);
    Task CompleteAsync(int id, string? notes);
}
