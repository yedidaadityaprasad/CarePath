namespace CarePath.Exceptions;

public class DuplicateValueException : DomainException
{
    public DuplicateValueException(string entityName, string propertyName, string displayName, string value)
        : base($"A {entityName.ToLowerInvariant()} with {displayName} '{value}' already exists.", propertyName)
    {
        Value = value;
    }

    public string Value { get; }

    public override string Title => "Duplicate value";
}
