namespace CarePath.Models.Entities;

public class InvoiceCoverageSnapshot : Entity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public int InsuranceCoverageId { get; set; }
    public string PayerCode { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string? GroupNumber { get; set; }
    public decimal CoveragePercent { get; set; }
    public decimal CoveredAmount { get; set; }
    public DateTime CapturedAt { get; set; }
}
