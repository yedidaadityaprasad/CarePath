using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class AdmissionIndexViewModel
{
    public AdmissionStatus? Status { get; set; } = AdmissionStatus.Open;

    [Display(Name = "Ward")]
    public int? WardId { get; set; }

    [StringLength(100)]
    [Display(Name = "Patient, MRN or admission #")]
    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public PagedResult<Admission> Results { get; set; } = PagedResult<Admission>.Empty();

    [ValidateNever]
    public IEnumerable<SelectListItem> Wards { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class AdmitPatientViewModel
{
    [Required]
    [Display(Name = "Patient")]
    public int? PatientId { get; set; }

    [Required]
    [Display(Name = "Bed")]
    public int? BedId { get; set; }

    [Required]
    [Display(Name = "Attending physician")]
    public int? AttendingPractitionerId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Admitted at")]
    public DateTime? AdmittedAt { get; set; }

    [Required, StringLength(500, MinimumLength = 3)]
    [Display(Name = "Reason for admission")]
    public string? ReasonForAdmission { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Diagnosis code")]
    public string? DiagnosisCode { get; set; }

    [Required, StringLength(300)]
    [Display(Name = "Diagnosis description")]
    public string? DiagnosisDescription { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Patients { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Beds { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Physicians { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class AdmissionEditViewModel
{
    public int Id { get; set; }
    public string AdmissionNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Attending physician")]
    public int? AttendingPractitionerId { get; set; }

    [Required, StringLength(500, MinimumLength = 3)]
    [Display(Name = "Reason for admission")]
    public string? ReasonForAdmission { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Physicians { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class TransferViewModel
{
    public int AdmissionId { get; set; }
    public string AdmissionNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string CurrentBed { get; set; } = string.Empty;

    [Required]
    [Display(Name = "New bed")]
    public int? NewBedId { get; set; }

    [Required, StringLength(500, MinimumLength = 3)]
    [Display(Name = "Reason for transfer")]
    public string? Reason { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Beds { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class DischargeViewModel
{
    public int AdmissionId { get; set; }
    public string AdmissionNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime AdmittedAt { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Discharged at")]
    public DateTime? DischargedAt { get; set; }

    [Required, StringLength(100)]
    public string? Disposition { get; set; }

    [Required, StringLength(300)]
    [Display(Name = "Final diagnosis")]
    public string? FinalDiagnosis { get; set; }

    [Required, StringLength(4000, MinimumLength = 10)]
    [Display(Name = "Discharge summary")]
    public string? Summary { get; set; }

    [StringLength(2000)]
    [Display(Name = "Follow-up instructions")]
    public string? FollowUpInstructions { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Dispositions { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class AdmissionNoteViewModel
{
    public int AdmissionId { get; set; }
    public string AdmissionNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 3)]
    [Display(Name = "Note")]
    public string? Text { get; set; }
}

public class VitalsViewModel
{
    public int AdmissionId { get; set; }
    public string AdmissionNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;

    [Required, Range(30, 45)]
    [Display(Name = "Temperature (°C)")]
    public decimal? TemperatureCelsius { get; set; }

    [Required, Range(20, 250)]
    [Display(Name = "Pulse (bpm)")]
    public int? PulseBpm { get; set; }

    [Required, Range(50, 260)]
    [Display(Name = "Systolic BP")]
    public int? SystolicBp { get; set; }

    [Required, Range(20, 160)]
    [Display(Name = "Diastolic BP")]
    public int? DiastolicBp { get; set; }

    [Required, Range(4, 60)]
    [Display(Name = "Respiratory rate")]
    public int? RespiratoryRate { get; set; }

    [Required, Range(50, 100)]
    [Display(Name = "SpO2 (%)")]
    public int? OxygenSaturation { get; set; }
}

public class AdmissionDetailsViewModel
{
    public Admission Admission { get; set; } = null!;
    public int LengthOfStayDays { get; set; }
}
