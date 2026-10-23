namespace CarePath.Models.Entities;

public class PractitionerStatusChange : Entity
{
    public int PractitionerId { get; set; }
    public Practitioner Practitioner { get; set; } = null!;
    public bool IsActive { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}
