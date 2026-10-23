namespace CarePath.Models.Entities;

public class EmergencyContact : Entity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
