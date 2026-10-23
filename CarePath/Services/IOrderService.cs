using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IOrderService
{
    Task<PagedResult<ClinicalOrder>> SearchAsync(OrderType? type, OrderStatus? status, string? search, int page, int pageSize = 25);
    Task<OrderDetailsViewModel> GetDetailsAsync(int id);
    Task<int> PlaceLabOrderAsync(LabOrderFormViewModel model);
    Task<int> PlaceMedicationOrderAsync(MedicationOrderFormViewModel model);
    Task<OrderEditViewModel> GetForEditAsync(int id);
    Task UpdateAsync(OrderEditViewModel model);
    Task CancelAsync(int id, string reason);
}
