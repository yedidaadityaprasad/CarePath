using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IPatientService
{
    Task<PagedResult<Patient>> SearchAsync(string? search, bool includeInactive, int page, int pageSize = 25);
    Task<PatientDetailsViewModel> GetDetailsAsync(int id);
    Task<PatientFormViewModel> GetForEditAsync(int id);
    Task<int> RegisterAsync(PatientFormViewModel model);
    Task UpdateAsync(PatientFormViewModel model);
    Task DeactivateAsync(int id, string reason);
    Task ReactivateAsync(int id);
    Task<AllergyFormViewModel> GetAllergyFormAsync(int patientId);
    Task AddAllergyAsync(AllergyFormViewModel model);
}
