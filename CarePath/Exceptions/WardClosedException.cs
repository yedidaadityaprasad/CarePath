namespace CarePath.Exceptions;

public class WardClosedException : DomainException
{
    public WardClosedException(string wardName, string? propertyName = null)
        : base($"Ward {wardName} is closed.", propertyName)
    {
    }

    public override string Title => "Ward closed";
}
