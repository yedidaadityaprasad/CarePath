namespace CarePath.Exceptions;

public class AdmissionClosedException : DomainException
{
    public AdmissionClosedException(string admissionNumber, string? propertyName = "AdmissionId")
        : base($"Admission {admissionNumber} has been discharged and cannot accept changes or new orders.", propertyName)
    {
    }

    public override string Title => "Admission closed";
}
