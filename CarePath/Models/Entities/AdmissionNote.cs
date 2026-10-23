namespace CarePath.Models.Entities;

public class AdmissionNote : Entity
{
    public int AdmissionId { get; set; }
    public Admission Admission { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public DateTime WrittenAt { get; set; }
    public string Author { get; set; } = string.Empty;
}
