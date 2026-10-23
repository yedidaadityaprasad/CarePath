namespace CarePath.Models.Entities;

public class Payer : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ClaimsEmail { get; set; }
    public decimal DefaultCoveragePercent { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<InsuranceCoverage> Coverages { get; set; } = new List<InsuranceCoverage>();
}
