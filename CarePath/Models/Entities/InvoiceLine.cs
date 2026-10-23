namespace CarePath.Models.Entities;

public class InvoiceLine : Entity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public InvoiceLineType LineType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public int? ClinicalOrderId { get; set; }
    public ClinicalOrder? ClinicalOrder { get; set; }
}
