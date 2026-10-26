using Microsoft.AspNetCore.Http;

namespace CarePath.Exceptions;

/// <summary>
/// Base type for business-rule violations. The message is safe to show to end users.
/// .NET 10: Inherits from Exception — no changes required.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message, string? propertyName = null)
        : base(message)
    {
        PropertyName = propertyName;
    }

    /// <summary>View model property the error belongs to, or null for a model-level error.</summary>
    public string? PropertyName { get; }

    public virtual string Title => "The request could not be completed";

    public virtual int StatusCode => StatusCodes.Status409Conflict;
}
