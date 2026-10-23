namespace CarePath.Models.Entities;

public class DischargeSummary : Entity
{
    public int AdmissionId { get; set; }
    public Admission Admission { get; set; } = null!;
    public string Disposition { get; set; } = string.Empty;
    public string FinalDiagnosis { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? FollowUpInstructions { get; set; }
    public DateTime WrittenAt { get; set; }
    public string WrittenBy { get; set; } = string.Empty;
}
