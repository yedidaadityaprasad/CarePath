namespace CarePath.Exceptions;

public class CoverageConflictException : DomainException
{
    public CoverageConflictException(string payerName, DateTime existingFrom, DateTime? existingTo)
        : base($"The patient already has active {payerName} coverage from {existingFrom:yyyy-MM-dd} to {(existingTo.HasValue ? existingTo.Value.ToString("yyyy-MM-dd") : "open-ended")} that overlaps these dates.", "EffectiveFrom")
    {
    }

    public override string Title => "Overlapping coverage";
}
