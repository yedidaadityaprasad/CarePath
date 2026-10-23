using Microsoft.AspNetCore.Http;

namespace CarePath.Exceptions;

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key, string? propertyName = null)
        : base($"{entityName} '{key}' was not found.", propertyName)
    {
        EntityName = entityName;
        Key = key;
    }

    public string EntityName { get; }
    public object Key { get; }

    public override string Title => "Record not found";
    public override int StatusCode => StatusCodes.Status404NotFound;
}
