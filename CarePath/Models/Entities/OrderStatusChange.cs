namespace CarePath.Models.Entities;

public class OrderStatusChange : Entity
{
    public int ClinicalOrderId { get; set; }
    public ClinicalOrder Order { get; set; } = null!;
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Note { get; set; }
}
