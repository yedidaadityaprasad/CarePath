namespace CarePath.Exceptions;

public class PatientInactiveException : DomainException
{
    public PatientInactiveException(string medicalRecordNumber, string? propertyName = "PatientId")
        : base($"Patient {medicalRecordNumber} is deactivated and cannot be admitted or booked.", propertyName)
    {
    }

    public override string Title => "Patient is inactive";
}
