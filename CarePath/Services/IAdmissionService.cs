using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IAdmissionService
{
    Task<PagedResult<Admission>> SearchAsync(AdmissionStatus? status, int? wardId, string? search, int page, int pageSize = 25);
    Task<AdmissionDetailsViewModel> GetDetailsAsync(int id);
    Task<int> AdmitAsync(AdmitPatientViewModel model);
    Task<AdmissionEditViewModel> GetForEditAsync(int id);
    Task UpdateAsync(AdmissionEditViewModel model);
    Task<TransferViewModel> GetTransferFormAsync(int id);
    Task TransferAsync(TransferViewModel model);
    Task<DischargeViewModel> GetDischargeFormAsync(int id);

    /// <summary>Discharges the admission, frees the bed and creates the draft invoice. Returns the invoice id.</summary>
    Task<int> DischargeAsync(DischargeViewModel model);

    Task<AdmissionNoteViewModel> GetNoteFormAsync(int id);
    Task AddNoteAsync(AdmissionNoteViewModel model);
    Task<VitalsViewModel> GetVitalsFormAsync(int id);
    Task RecordVitalsAsync(VitalsViewModel model);
}
