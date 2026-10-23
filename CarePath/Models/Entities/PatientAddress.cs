namespace CarePath.Models.Entities;

public class PatientAddress : Entity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
