using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface ILabService
{
    Task<IReadOnlyList<ClinicalOrder>> GetWorklistAsync(OrderStatus? status);
    Task<LabOrderViewModel> GetOrderAsync(int orderId);
    Task AcceptAsync(int orderId);
    Task<SpecimenFormViewModel> GetSpecimenFormAsync(int orderId);
    Task RecordSpecimenAsync(SpecimenFormViewModel model);
    Task<LabResultFormViewModel> GetResultFormAsync(int orderId);
    Task EnterResultAsync(LabResultFormViewModel model);
}
