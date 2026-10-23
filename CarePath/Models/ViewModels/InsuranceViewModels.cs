using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class PayerIndexViewModel
{
    [Display(Name = "Include inactive")]
    public bool IncludeInactive { get; set; }

    public IReadOnlyList<Payer> Payers { get; set; } = Array.Empty<Payer>();
}

public class PayerFormViewModel
{
    public int Id { get; set; }

    [Required]
    [RegularExpression("^[A-Za-z0-9-]{2,20}$", ErrorMessage = "Code must be 2-20 letters, digits or dashes.")]
    public string? Code { get; set; }

    [Required, StringLength(150)]
    public string? Name { get; set; }

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(200)]
    [Display(Name = "Claims email")]
    public string? ClaimsEmail { get; set; }

    [Required]
    [Range(0, 100)]
    [Display(Name = "Default coverage %")]
    public decimal? DefaultCoveragePercent { get; set; } = 80;

    public bool IsEdit => Id > 0;
}

public class PayerDetailsViewModel
{
    public Payer Payer { get; set; } = null!;
    public int ActiveCoverageCount { get; set; }
}

public class CoverageIndexViewModel
{
    [Display(Name = "Patient, MRN or member id")]
    [StringLength(100)]
    public string? Search { get; set; }

    [Display(Name = "Active only")]
    public bool ActiveOnly { get; set; } = true;

    public int Page { get; set; } = 1;

    public PagedResult<InsuranceCoverage> Results { get; set; } = PagedResult<InsuranceCoverage>.Empty();
}

public class CoverageFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Patient")]
    public int? PatientId { get; set; }

    public string? PatientName { get; set; }

    [Required]
    [Display(Name = "Payer")]
    public int? PayerId { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Member id")]
    public string? MemberId { get; set; }

    [StringLength(50)]
    [Display(Name = "Group number")]
    public string? GroupNumber { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Effective from")]
    public DateTime? EffectiveFrom { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Effective to")]
    public DateTime? EffectiveTo { get; set; }

    [Range(0, 100)]
    [Display(Name = "Coverage % (blank uses payer default)")]
    public decimal? CoveragePercent { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Patients { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Payers { get; set; } = Enumerable.Empty<SelectListItem>();

    public bool IsEdit => Id > 0;
}

public class CoverageDetailsViewModel
{
    public InsuranceCoverage Coverage { get; set; } = null!;
    public int AdmissionCount { get; set; }
}
