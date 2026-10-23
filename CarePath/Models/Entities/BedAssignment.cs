namespace CarePath.Models.Entities;

public class BedAssignment : Entity
{
    public int AdmissionId { get; set; }
    public Admission Admission { get; set; } = null!;
    public int BedId { get; set; }
    public Bed Bed { get; set; } = null!;
    public DateTime AssignedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string AssignedBy { get; set; } = string.Empty;
}
