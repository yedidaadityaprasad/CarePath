namespace CarePath.Exceptions;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string drugName, string wardName, int onHand, int requested)
        : base($"Insufficient stock of {drugName} on {wardName}: {onHand} on hand, {requested} requested.")
    {
        OnHand = onHand;
        Requested = requested;
    }

    public int OnHand { get; }
    public int Requested { get; }

    public override string Title => "Insufficient stock";
}
