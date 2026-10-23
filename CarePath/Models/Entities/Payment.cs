namespace CarePath.Models.Entities;

public class Payment : Entity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Reference { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
}
