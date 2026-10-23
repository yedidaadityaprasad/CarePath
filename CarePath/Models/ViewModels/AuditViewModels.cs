using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class AuditIndexViewModel
{
    [Display(Name = "Entity")]
    [StringLength(100)]
    public string? EntityName { get; set; }

    [Display(Name = "Entity id")]
    [StringLength(50)]
    public string? EntityId { get; set; }

    [Display(Name = "User")]
    [StringLength(256)]
    public string? UserName { get; set; }

    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [DataType(DataType.Date)]
    public DateTime? To { get; set; }

    public int Page { get; set; } = 1;

    public PagedResult<AuditEntry> Results { get; set; } = PagedResult<AuditEntry>.Empty();

    [ValidateNever]
    public IEnumerable<SelectListItem> EntityNames { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class AuditDetailsViewModel
{
    public AuditEntry Entry { get; set; } = null!;
    public IReadOnlyList<AuditEntry> RelatedEntries { get; set; } = Array.Empty<AuditEntry>();
}
