namespace CarePath.Exceptions;

public class ResultAlreadyRecordedException : DomainException
{
    public ResultAlreadyRecordedException(string orderNumber)
        : base($"A result has already been entered for lab order {orderNumber}.")
    {
    }

    public override string Title => "Result already recorded";
}
