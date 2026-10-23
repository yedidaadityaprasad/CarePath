namespace CarePath.Models.Entities;

public class Shift : Entity
{
    public int PractitionerId { get; set; }
    public Practitioner Practitioner { get; set; } = null!;
    public int WardId { get; set; }
    public Ward Ward { get; set; } = null!;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public ShiftStatus Status { get; set; } = ShiftStatus.Scheduled;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public bool Overlaps(DateTime start, DateTime end) => StartsAt < end && start < EndsAt;
}
