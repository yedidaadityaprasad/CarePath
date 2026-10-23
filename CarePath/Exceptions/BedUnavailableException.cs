using CarePath.Models.Entities;

namespace CarePath.Exceptions;

public class BedUnavailableException : DomainException
{
    public BedUnavailableException(string bedLabel, BedStatus status, string? propertyName = "BedId")
        : base($"Bed {bedLabel} is {status} and cannot be assigned.", propertyName)
    {
        Status = status;
    }

    public BedStatus Status { get; }

    public override string Title => "Bed unavailable";
}
