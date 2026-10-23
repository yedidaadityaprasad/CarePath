namespace CarePath.Models.Entities;

public class AppointmentStatusChange : Entity
{
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public AppointmentStatus FromStatus { get; set; }
    public AppointmentStatus ToStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Note { get; set; }
}
