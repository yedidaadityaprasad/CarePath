namespace CarePath.Models.Entities;

public class InsuranceCoverage : Entity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int PayerId { get; set; }
    public Payer Payer { get; set; } = null!;
    public string MemberId { get; set; } = string.Empty;
    public string? GroupNumber { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public decimal CoveragePercent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? TerminatedAt { get; set; }
    public string? TerminationReason { get; set; }

    public bool CoversDate(DateTime date) =>
        IsActive && EffectiveFrom.Date <= date.Date && (EffectiveTo == null || EffectiveTo.Value.Date >= date.Date);

    public bool Overlaps(DateTime from, DateTime? to)
    {
        var thisEnd = EffectiveTo?.Date ?? DateTime.MaxValue.Date;
        var otherEnd = to?.Date ?? DateTime.MaxValue.Date;
        return EffectiveFrom.Date <= otherEnd && from.Date <= thisEnd;
    }
}
