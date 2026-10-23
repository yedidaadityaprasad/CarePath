using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class DrugIndexViewModel
{
    [StringLength(100)]
    [Display(Name = "Code or name")]
    public string? Search { get; set; }

    [Display(Name = "Include inactive")]
    public bool IncludeInactive { get; set; }

    public IReadOnlyList<Drug> Drugs { get; set; } = Array.Empty<Drug>();
}

public class DrugFormViewModel
{
    public int Id { get; set; }

    [Required]
    [RegularExpression("^[A-Za-z0-9-]{2,20}$", ErrorMessage = "Code must be 2-20 letters, digits or dashes.")]
    public string? Code { get; set; }

    [Required, StringLength(150)]
    public string? Name { get; set; }

    [Required]
    [Display(Name = "Category")]
    public int? DrugCategoryId { get; set; }

    [Required, StringLength(50)]
    public string? Strength { get; set; }

    [Required, StringLength(50)]
    public string? Form { get; set; }

    [Required, Range(0, 1000000)]
    [DataType(DataType.Currency)]
    [Display(Name = "Unit price")]
    public decimal? UnitPrice { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();

    public bool IsEdit => Id > 0;
}

public class DrugDetailsViewModel
{
    public Drug Drug { get; set; } = null!;
    public IReadOnlyList<WardStock> Stock { get; set; } = Array.Empty<WardStock>();
}

public class WardStockIndexViewModel
{
    [Display(Name = "Ward")]
    public int? WardId { get; set; }

    [Display(Name = "At or below reorder level only")]
    public bool LowStockOnly { get; set; }

    public IReadOnlyList<WardStock> Stock { get; set; } = Array.Empty<WardStock>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Wards { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class WardStockFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Ward")]
    public int? WardId { get; set; }

    [Required]
    [Display(Name = "Drug")]
    public int? DrugId { get; set; }

    [Required, Range(0, 100000)]
    [Display(Name = "Opening quantity")]
    public int? InitialQuantity { get; set; } = 0;

    [Required, Range(0, 100000)]
    [Display(Name = "Reorder level")]
    public int? ReorderLevel { get; set; } = 10;

    [ValidateNever]
    public IEnumerable<SelectListItem> Wards { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Drugs { get; set; } = Enumerable.Empty<SelectListItem>();

    public bool IsEdit => Id > 0;
}

public class WardStockDetailsViewModel
{
    public WardStock Stock { get; set; } = null!;
    public IReadOnlyList<StockMovement> Movements { get; set; } = Array.Empty<StockMovement>();
}

public class RestockViewModel
{
    public int StockId { get; set; }
    public string WardName { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }

    [Required, Range(1, 100000)]
    [Display(Name = "Quantity received")]
    public int? Quantity { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Delivery reference")]
    public string? Reference { get; set; }
}

public class DispenseQueueItem
{
    public ClinicalOrder Order { get; set; } = null!;
    public Ward Ward { get; set; } = null!;
    public int? QuantityOnHand { get; set; }
    public bool CanDispense => QuantityOnHand.HasValue && QuantityOnHand.Value >= (Order.MedicationDetail?.Quantity ?? int.MaxValue);
}

public class DispenseQueueViewModel
{
    public IReadOnlyList<DispenseQueueItem> Items { get; set; } = Array.Empty<DispenseQueueItem>();
}

public class DispenseViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string WardName { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int? QuantityOnHand { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
