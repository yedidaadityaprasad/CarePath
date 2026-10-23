namespace CarePath.Models.Entities;

public class Specimen : Entity
{
    public int LabOrderDetailId { get; set; }
    public LabOrderDetail LabOrder { get; set; } = null!;
    public int SpecimenTypeId { get; set; }
    public SpecimenType SpecimenType { get; set; } = null!;
    public string AccessionNumber { get; set; } = string.Empty;
    public DateTime CollectedAt { get; set; }
    public string CollectedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
