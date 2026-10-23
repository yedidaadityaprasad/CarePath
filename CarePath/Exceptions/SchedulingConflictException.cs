namespace CarePath.Exceptions;

public class SchedulingConflictException : DomainException
{
    public SchedulingConflictException(string message, string? propertyName = "StartsAt")
        : base(message, propertyName)
    {
    }

    public override string Title => "Scheduling conflict";
}
