using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class PractitionerIndexViewModel
{
    [StringLength(100)]
    [Display(Name = "Name or license")]
    public string? Search { get; set; }

    public PractitionerType? Type { get; set; }

    [Display(Name = "Active only")]
    public bool ActiveOnly { get; set; }

    public IReadOnlyList<Practitioner> Practitioners { get; set; } = Array.Empty<Practitioner>();
}

public class PractitionerFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "First name")]
    public string? FirstName { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    [Required]
    public PractitionerType? Type { get; set; }

    [Required]
    [Display(Name = "License number")]
    [RegularExpression("^[A-Za-z0-9/-]{4,30}$", ErrorMessage = "License number must be 4-30 letters, digits, dashes or slashes.")]
    public string? LicenseNumber { get; set; }

    [Display(Name = "Specialty")]
    public int? SpecialtyId { get; set; }

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Specialties { get; set; } = Enumerable.Empty<SelectListItem>();

    public bool IsEdit => Id > 0;
}

public class PractitionerDetailsViewModel
{
    public Practitioner Practitioner { get; set; } = null!;
    public IReadOnlyList<Shift> UpcomingShifts { get; set; } = Array.Empty<Shift>();
    public IReadOnlyList<Appointment> UpcomingAppointments { get; set; } = Array.Empty<Appointment>();
    public int OpenAdmissionCount { get; set; }
}
