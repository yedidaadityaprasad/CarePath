namespace CarePath.Models.Entities;

public class InvoiceStatusChange : Entity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public InvoiceStatus FromStatus { get; set; }
    public InvoiceStatus ToStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Note { get; set; }
}
