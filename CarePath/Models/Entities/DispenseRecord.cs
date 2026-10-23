namespace CarePath.Models.Entities;

public class DispenseRecord : Entity
{
    public int ClinicalOrderId { get; set; }
    public ClinicalOrder Order { get; set; } = null!;
    public int WardStockId { get; set; }
    public WardStock WardStock { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime DispensedAt { get; set; }
    public string DispensedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
