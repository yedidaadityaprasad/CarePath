namespace CarePath.Models.Entities;

public class PatientAllergy : Entity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public string Substance { get; set; } = string.Empty;
    public string? Reaction { get; set; }
    public AllergySeverity Severity { get; set; }
    public DateTime RecordedAt { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
}
