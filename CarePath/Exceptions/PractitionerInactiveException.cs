namespace CarePath.Exceptions;

public class PractitionerInactiveException : DomainException
{
    public PractitionerInactiveException(string practitionerName, string? propertyName = "PractitionerId")
        : base($"{practitionerName} is inactive and cannot be assigned.", propertyName)
    {
    }

    public override string Title => "Practitioner is inactive";
}
