namespace CarePath.Models.Entities;

public class StockMovement : Entity
{
    public int WardStockId { get; set; }
    public WardStock WardStock { get; set; } = null!;
    public StockMovementType Type { get; set; }
    public int QuantityChange { get; set; }
    public int QuantityAfter { get; set; }
    public DateTime OccurredAt { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}
