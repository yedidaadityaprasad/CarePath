using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class LabWorklistViewModel
{
    public OrderStatus? Status { get; set; }

    public IReadOnlyList<ClinicalOrder> Orders { get; set; } = Array.Empty<ClinicalOrder>();
}

public class LabOrderViewModel
{
    public ClinicalOrder Order { get; set; } = null!;
    public LabOrderDetail Detail => Order.LabDetail!;
}

public class SpecimenFormViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string SpecimenTypeName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Collected at")]
    public DateTime? CollectedAt { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class LabResultFormViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public ResultValueType ResultType { get; set; }
    public string ReferenceRange { get; set; } = string.Empty;
    public string? Unit { get; set; }

    [Display(Name = "Numeric value")]
    [Range(-1000000, 1000000)]
    public decimal? NumericValue { get; set; }

    [Display(Name = "Text value")]
    [StringLength(500)]
    public string? TextValue { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }
}

public class LabTestIndexViewModel
{
    [Display(Name = "Include inactive")]
    public bool IncludeInactive { get; set; }

    public IReadOnlyList<LabTestDefinition> Tests { get; set; } = Array.Empty<LabTestDefinition>();
}

public class LabTestFormViewModel
{
    public int Id { get; set; }

    [Required]
    [RegularExpression("^[A-Za-z0-9-]{2,20}$", ErrorMessage = "Code must be 2-20 letters, digits or dashes.")]
    public string? Code { get; set; }

    [Required, StringLength(150)]
    public string? Name { get; set; }

    [Required]
    [Display(Name = "Specimen type")]
    public int? SpecimenTypeId { get; set; }

    [Required]
    [Display(Name = "Result type")]
    public ResultValueType? ResultType { get; set; }

    [StringLength(30)]
    public string? Unit { get; set; }

    [Display(Name = "Reference low")]
    public decimal? ReferenceLow { get; set; }

    [Display(Name = "Reference high")]
    public decimal? ReferenceHigh { get; set; }

    [StringLength(200)]
    [Display(Name = "Expected text result")]
    public string? ReferenceText { get; set; }

    [Required, Range(0, 1000000)]
    [DataType(DataType.Currency)]
    public decimal? Price { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> SpecimenTypes { get; set; } = Enumerable.Empty<SelectListItem>();

    public bool IsEdit => Id > 0;
}

public class LabTestDetailsViewModel
{
    public LabTestDefinition Test { get; set; } = null!;
    public int OrderCount { get; set; }
}
