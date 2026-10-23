namespace CarePath.Exceptions;

public class CoverageRequiredException : DomainException
{
    public CoverageRequiredException(string medicalRecordNumber, DateTime date, string? propertyName = "PatientId")
        : base($"Patient {medicalRecordNumber} has no active insurance coverage on {date:yyyy-MM-dd}.", propertyName)
    {
    }

    public override string Title => "Active coverage required";
}
