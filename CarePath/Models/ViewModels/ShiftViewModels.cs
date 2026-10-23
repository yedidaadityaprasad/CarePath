using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class ShiftIndexViewModel
{
    [Display(Name = "Ward")]
    public int? WardId { get; set; }

    [Display(Name = "Practitioner")]
    public int? PractitionerId { get; set; }

    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [DataType(DataType.Date)]
    public DateTime? To { get; set; }

    [Display(Name = "Include cancelled")]
    public bool IncludeCancelled { get; set; }

    public IReadOnlyList<Shift> Shifts { get; set; } = Array.Empty<Shift>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Wards { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Practitioners { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class ShiftFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Practitioner")]
    public int? PractitionerId { get; set; }

    [Required]
    [Display(Name = "Ward")]
    public int? WardId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Starts at")]
    public DateTime? StartsAt { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Ends at")]
    public DateTime? EndsAt { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Practitioners { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Wards { get; set; } = Enumerable.Empty<SelectListItem>();

    public bool IsEdit => Id > 0;
}

public class ShiftDetailsViewModel
{
    public Shift Shift { get; set; } = null!;
    public bool CanEdit { get; set; }
    public bool CanCancel { get; set; }
}
