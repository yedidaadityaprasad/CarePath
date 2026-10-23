namespace CarePath.Models.Entities;

public class Bed : Entity
{
    public int WardId { get; set; }
    public Ward Ward { get; set; } = null!;
    public string BedNumber { get; set; } = string.Empty;
    public BedStatus Status { get; set; } = BedStatus.Available;
    public string? Notes { get; set; }

    public ICollection<BedStatusChange> StatusChanges { get; set; } = new List<BedStatusChange>();
    public ICollection<BedAssignment> Assignments { get; set; } = new List<BedAssignment>();

    public bool IsAssignable => Status == BedStatus.Available && Ward is { IsClosed: false };

    public void ChangeStatus(BedStatus newStatus, DateTime at, string changedBy, string reason)
    {
        StatusChanges.Add(new BedStatusChange
        {
            Bed = this,
            FromStatus = Status,
            ToStatus = newStatus,
            ChangedAt = at,
            ChangedBy = changedBy,
            Reason = reason
        });
        Status = newStatus;
    }
}
