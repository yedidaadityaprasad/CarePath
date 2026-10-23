namespace CarePath.Models.Entities;

public class ClinicalOrder : Entity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int AdmissionId { get; set; }
    public Admission Admission { get; set; } = null!;
    public int OrderingPractitionerId { get; set; }
    public Practitioner OrderingPractitioner { get; set; } = null!;
    public OrderType Type { get; set; }
    public OrderPriority Priority { get; set; } = OrderPriority.Routine;
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public string? Notes { get; set; }
    public DateTime PlacedAt { get; set; }
    public string PlacedBy { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public LabOrderDetail? LabDetail { get; set; }
    public MedicationOrderDetail? MedicationDetail { get; set; }
    public DispenseRecord? Dispense { get; set; }
    public ICollection<OrderStatusChange> StatusChanges { get; set; } = new List<OrderStatusChange>();

    public string Description => Type == OrderType.Lab
        ? LabDetail?.TestDefinition?.Name ?? "Lab test"
        : MedicationDetail?.Drug?.Name ?? "Medication";

    public void ChangeStatus(OrderStatus newStatus, DateTime at, string changedBy, string? note)
    {
        StatusChanges.Add(new OrderStatusChange
        {
            Order = this,
            FromStatus = Status,
            ToStatus = newStatus,
            ChangedAt = at,
            ChangedBy = changedBy,
            Note = note
        });
        Status = newStatus;
        switch (newStatus)
        {
            case OrderStatus.InProgress:
                StartedAt = at;
                break;
            case OrderStatus.Completed:
                StartedAt ??= at;
                CompletedAt = at;
                break;
            case OrderStatus.Cancelled:
                CancelledAt = at;
                CancellationReason = note;
                break;
        }
    }
}
