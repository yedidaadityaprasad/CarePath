using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IBillingService
{
    Task<PagedResult<Invoice>> SearchAsync(InvoiceStatus? status, string? search, int page, int pageSize = 25);
    Task<InvoiceDetailsViewModel> GetDetailsAsync(int id);

    /// <summary>Builds the draft invoice for a discharged admission. Returns the invoice id.</summary>
    Task<int> CreateInvoiceForDischargeAsync(int admissionId);

    Task IssueAsync(int id, int dueInDays);
    Task<PaymentViewModel> GetPaymentFormAsync(int id);
    Task RecordPaymentAsync(PaymentViewModel model);
    Task VoidAsync(int id, string reason);
}
