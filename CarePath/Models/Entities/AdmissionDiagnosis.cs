namespace CarePath.Models.Entities;

public class AdmissionDiagnosis : Entity
{
    public int AdmissionId { get; set; }
    public Admission Admission { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime RecordedAt { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
}
