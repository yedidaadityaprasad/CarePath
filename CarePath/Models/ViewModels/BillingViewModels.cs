using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;

namespace CarePath.Models.ViewModels;

public class InvoiceIndexViewModel
{
    public InvoiceStatus? Status { get; set; }

    [StringLength(100)]
    [Display(Name = "Invoice #, MRN or patient")]
    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public PagedResult<Invoice> Results { get; set; } = PagedResult<Invoice>.Empty();
}

public class InvoiceDetailsViewModel
{
    public Invoice Invoice { get; set; } = null!;
    public IssueInvoiceViewModel Issue { get; set; } = new();
}

public class IssueInvoiceViewModel
{
    public int InvoiceId { get; set; }

    [Required, Range(1, 180)]
    [Display(Name = "Payment due in (days)")]
    public int? DueInDays { get; set; } = 30;
}

public class PaymentViewModel
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public decimal BalanceDue { get; set; }

    [Required, Range(typeof(decimal), "0.01", "100000000")]
    [DataType(DataType.Currency)]
    public decimal? Amount { get; set; }

    [Required]
    public PaymentMethod? Method { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }
}
