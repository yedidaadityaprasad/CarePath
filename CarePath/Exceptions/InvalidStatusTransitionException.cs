namespace CarePath.Exceptions;

public class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(string entityName, string currentStatus, string attemptedAction)
        : base($"Cannot {attemptedAction} this {entityName.ToLowerInvariant()} because it is {currentStatus}.")
    {
        CurrentStatus = currentStatus;
        AttemptedAction = attemptedAction;
    }

    public string CurrentStatus { get; }
    public string AttemptedAction { get; }

    public override string Title => "Action not allowed in the current status";
}
