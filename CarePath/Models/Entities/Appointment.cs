namespace CarePath.Models.Entities;

public class Appointment : Entity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int PractitionerId { get; set; }
    public Practitioner Practitioner { get; set; } = null!;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Location { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public DateTime BookedAt { get; set; }
    public string BookedBy { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public string? CompletionNotes { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public ICollection<AppointmentStatusChange> StatusChanges { get; set; } = new List<AppointmentStatusChange>();

    public int DurationMinutes => (int)(EndsAt - StartsAt).TotalMinutes;

    public void ChangeStatus(AppointmentStatus newStatus, DateTime at, string changedBy, string? note)
    {
        StatusChanges.Add(new AppointmentStatusChange
        {
            Appointment = this,
            FromStatus = Status,
            ToStatus = newStatus,
            ChangedAt = at,
            ChangedBy = changedBy,
            Note = note
        });
        Status = newStatus;
    }
}
