using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class WardSummary
{
    public Ward Ward { get; set; } = null!;
    public int TotalBeds { get; set; }
    public int Occupied { get; set; }
    public int Available { get; set; }
    public int OutOfService { get; set; }
    public decimal OccupancyPercent => TotalBeds == 0 ? 0 : Math.Round(Occupied * 100m / TotalBeds, 1);
}

public class WardIndexViewModel
{
    [Display(Name = "Include closed wards")]
    public bool IncludeClosed { get; set; } = true;

    public IReadOnlyList<WardSummary> Wards { get; set; } = Array.Empty<WardSummary>();
}

public class WardFormViewModel
{
    public int Id { get; set; }

    [Required]
    [RegularExpression("^[A-Za-z0-9-]{2,20}$", ErrorMessage = "Code must be 2-20 letters, digits or dashes.")]
    public string? Code { get; set; }

    [Required, StringLength(100)]
    public string? Name { get; set; }

    [Required]
    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [Required]
    [Range(-5, 100)]
    public int? Floor { get; set; }

    [Required]
    [Range(0, 1000000)]
    [DataType(DataType.Currency)]
    [Display(Name = "Daily rate")]
    public decimal? DailyRate { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Departments { get; set; } = Enumerable.Empty<SelectListItem>();

    public bool IsEdit => Id > 0;
}

public class WardDetailsViewModel
{
    public Ward Ward { get; set; } = null!;
    public IReadOnlyList<Bed> Beds { get; set; } = Array.Empty<Bed>();
    public IReadOnlyDictionary<int, Admission> OccupantsByBed { get; set; } = new Dictionary<int, Admission>();
}

public class BedIndexViewModel
{
    [Display(Name = "Ward")]
    public int? WardId { get; set; }

    public BedStatus? Status { get; set; }

    public IReadOnlyList<Bed> Beds { get; set; } = Array.Empty<Bed>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Wards { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class BedFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Ward")]
    public int? WardId { get; set; }

    [Required]
    [Display(Name = "Bed number")]
    [RegularExpression("^[A-Za-z0-9-]{1,20}$", ErrorMessage = "Bed number must be 1-20 letters, digits or dashes.")]
    public string? BedNumber { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Wards { get; set; } = Enumerable.Empty<SelectListItem>();

    public bool IsEdit => Id > 0;
}

public class BedDetailsViewModel
{
    public Bed Bed { get; set; } = null!;
    public Admission? CurrentAdmission { get; set; }
    public IReadOnlyList<BedStatusChange> History { get; set; } = Array.Empty<BedStatusChange>();
}
