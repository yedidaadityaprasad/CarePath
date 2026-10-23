namespace CarePath.Models.Entities;

public class WardStock : Entity
{
    public int WardId { get; set; }
    public Ward Ward { get; set; } = null!;
    public int DrugId { get; set; }
    public Drug Drug { get; set; } = null!;
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public DateTime? LastRestockedAt { get; set; }

    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();

    public bool BelowReorderLevel => QuantityOnHand <= ReorderLevel;

    public void Apply(StockMovementType type, int quantityChange, DateTime at, string performedBy, string reference)
    {
        QuantityOnHand += quantityChange;
        Movements.Add(new StockMovement
        {
            WardStock = this,
            Type = type,
            QuantityChange = quantityChange,
            QuantityAfter = QuantityOnHand,
            OccurredAt = at,
            PerformedBy = performedBy,
            Reference = reference
        });
    }
}
