using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Services;

/// <summary>Builds select lists for foreign-key fields on forms.</summary>
public interface ILookupService
{
    Task<IReadOnlyList<SelectListItem>> PatientsAsync(bool activeOnly = true);
    Task<IReadOnlyList<SelectListItem>> PractitionersAsync(PractitionerType? type = null, bool activeOnly = true);
    Task<IReadOnlyList<SelectListItem>> AvailableBedsAsync();
    Task<IReadOnlyList<SelectListItem>> WardsAsync(bool openOnly = false);
    Task<IReadOnlyList<SelectListItem>> PayersAsync(bool activeOnly = true);
    Task<IReadOnlyList<SelectListItem>> SpecialtiesAsync();
    Task<IReadOnlyList<SelectListItem>> DepartmentsAsync();
    Task<IReadOnlyList<SelectListItem>> SpecimenTypesAsync();
    Task<IReadOnlyList<SelectListItem>> DrugCategoriesAsync();
    Task<IReadOnlyList<SelectListItem>> DrugsAsync(bool activeOnly = true);
    Task<IReadOnlyList<SelectListItem>> LabTestsAsync(bool activeOnly = true);
    Task<IReadOnlyList<SelectListItem>> OpenAdmissionsAsync();
    IReadOnlyList<SelectListItem> Dispositions();
    IReadOnlyList<SelectListItem> Roles();
}
