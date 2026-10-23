using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;

namespace CarePath.Models.ViewModels;

public class PatientIndexViewModel
{
    [Display(Name = "MRN or name")]
    [StringLength(100)]
    public string? Search { get; set; }

    [Display(Name = "Include deactivated")]
    public bool IncludeInactive { get; set; }

    public int Page { get; set; } = 1;

    public PagedResult<Patient> Results { get; set; } = PagedResult<Patient>.Empty();
}

public class PatientFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Medical record number")]
    [RegularExpression("^[A-Za-z0-9-]{4,20}$", ErrorMessage = "MRN must be 4-20 letters, digits or dashes.")]
    public string? MedicalRecordNumber { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "First name")]
    public string? FirstName { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateTime? DateOfBirth { get; set; }

    [Required]
    public Gender? Gender { get; set; }

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [Required, StringLength(200)]
    [Display(Name = "Address line 1")]
    public string? AddressLine1 { get; set; }

    [StringLength(200)]
    [Display(Name = "Address line 2")]
    public string? AddressLine2 { get; set; }

    [Required, StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    [Display(Name = "State / region")]
    public string? State { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    [Required, StringLength(100)]
    public string? Country { get; set; } = "India";

    [StringLength(150)]
    [Display(Name = "Emergency contact name")]
    public string? ContactName { get; set; }

    [StringLength(50)]
    [Display(Name = "Relationship")]
    public string? ContactRelationship { get; set; }

    [Phone, StringLength(30)]
    [Display(Name = "Emergency contact phone")]
    public string? ContactPhone { get; set; }

    public bool IsEdit => Id > 0;
}

public class PatientDetailsViewModel
{
    public Patient Patient { get; set; } = null!;
    public IReadOnlyList<InsuranceCoverage> Coverages { get; set; } = Array.Empty<InsuranceCoverage>();
    public IReadOnlyList<Admission> Admissions { get; set; } = Array.Empty<Admission>();
    public IReadOnlyList<Appointment> UpcomingAppointments { get; set; } = Array.Empty<Appointment>();
    public Admission? OpenAdmission => Admissions.FirstOrDefault(a => a.Status == AdmissionStatus.Open);
}

public class AllergyFormViewModel
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string? Substance { get; set; }

    [StringLength(300)]
    public string? Reaction { get; set; }

    [Required]
    public AllergySeverity? Severity { get; set; }
}
