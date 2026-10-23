namespace CarePath.Models.Entities;

public class MedicationOrderDetail : Entity
{
    public int ClinicalOrderId { get; set; }
    public ClinicalOrder Order { get; set; } = null!;
    public int DrugId { get; set; }
    public Drug Drug { get; set; } = null!;
    public string Dose { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
