namespace CarePath.Models.Entities;

public class BedStatusChange : Entity
{
    public int BedId { get; set; }
    public Bed Bed { get; set; } = null!;
    public BedStatus FromStatus { get; set; }
    public BedStatus ToStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
