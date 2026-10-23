namespace CarePath.Models.Entities;

public class LabOrderDetail : Entity
{
    public int ClinicalOrderId { get; set; }
    public ClinicalOrder Order { get; set; } = null!;
    public int LabTestDefinitionId { get; set; }
    public LabTestDefinition TestDefinition { get; set; } = null!;
    public string? ClinicalIndication { get; set; }

    public Specimen? Specimen { get; set; }
    public LabResult? Result { get; set; }
}
