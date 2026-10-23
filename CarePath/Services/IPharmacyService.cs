using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IPharmacyService
{
    Task<IReadOnlyList<WardStock>> ListStockAsync(int? wardId, bool lowStockOnly);
    Task<WardStockDetailsViewModel> GetStockDetailsAsync(int id);
    Task<int> CreateStockAsync(WardStockFormViewModel model);
    Task<WardStockFormViewModel> GetStockForEditAsync(int id);
    Task UpdateStockAsync(WardStockFormViewModel model);
    Task<RestockViewModel> GetRestockFormAsync(int stockId);
    Task RestockAsync(RestockViewModel model);
    Task<DispenseQueueViewModel> GetDispenseQueueAsync();
    Task<DispenseViewModel> GetDispenseFormAsync(int orderId);
    Task DispenseAsync(int orderId, string? notes);
}
