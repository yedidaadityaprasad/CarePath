namespace CarePath.Models.Entities;

public class VitalSign : Entity
{
    public int AdmissionId { get; set; }
    public Admission Admission { get; set; } = null!;
    public DateTime RecordedAt { get; set; }
    public decimal TemperatureCelsius { get; set; }
    public int PulseBpm { get; set; }
    public int SystolicBp { get; set; }
    public int DiastolicBp { get; set; }
    public int RespiratoryRate { get; set; }
    public int OxygenSaturation { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
}
