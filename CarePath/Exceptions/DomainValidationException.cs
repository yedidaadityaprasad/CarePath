using Microsoft.AspNetCore.Http;

namespace CarePath.Exceptions;

public class DomainValidationException : DomainException
{
    public DomainValidationException(string message, string? propertyName = null)
        : base(message, propertyName)
    {
    }

    public override string Title => "Invalid request";
    public override int StatusCode => StatusCodes.Status400BadRequest;
}
