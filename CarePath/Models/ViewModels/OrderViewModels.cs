using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class OrderIndexViewModel
{
    public OrderType? Type { get; set; }

    public OrderStatus? Status { get; set; }

    [StringLength(100)]
    [Display(Name = "Order #, admission # or patient")]
    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public PagedResult<ClinicalOrder> Results { get; set; } = PagedResult<ClinicalOrder>.Empty();
}

public class LabOrderFormViewModel
{
    [Required]
    [Display(Name = "Admission")]
    public int? AdmissionId { get; set; }

    [Required]
    [Display(Name = "Ordering practitioner")]
    public int? OrderingPractitionerId { get; set; }

    [Required]
    [Display(Name = "Test")]
    public int? LabTestDefinitionId { get; set; }

    [Required]
    public OrderPriority? Priority { get; set; } = OrderPriority.Routine;

    [StringLength(500)]
    [Display(Name = "Clinical indication")]
    public string? ClinicalIndication { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Admissions { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Practitioners { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Tests { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class MedicationOrderFormViewModel
{
    [Required]
    [Display(Name = "Admission")]
    public int? AdmissionId { get; set; }

    [Required]
    [Display(Name = "Ordering practitioner")]
    public int? OrderingPractitionerId { get; set; }

    [Required]
    [Display(Name = "Drug")]
    public int? DrugId { get; set; }

    [Required, StringLength(50)]
    public string? Dose { get; set; }

    [Required, StringLength(50)]
    public string? Route { get; set; }

    [Required, StringLength(50)]
    public string? Frequency { get; set; }

    [Required, Range(1, 1000)]
    [Display(Name = "Quantity to dispense")]
    public int? Quantity { get; set; }

    [Required]
    public OrderPriority? Priority { get; set; } = OrderPriority.Routine;

    [StringLength(1000)]
    public string? Notes { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Admissions { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Practitioners { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Drugs { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class OrderEditViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderType Type { get; set; }
    public string Description { get; set; } = string.Empty;

    [Required]
    public OrderPriority? Priority { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(50)]
    public string? Dose { get; set; }

    [StringLength(50)]
    public string? Route { get; set; }

    [StringLength(50)]
    public string? Frequency { get; set; }

    [Range(1, 1000)]
    [Display(Name = "Quantity to dispense")]
    public int? Quantity { get; set; }
}

public class OrderDetailsViewModel
{
    public ClinicalOrder Order { get; set; } = null!;
}
