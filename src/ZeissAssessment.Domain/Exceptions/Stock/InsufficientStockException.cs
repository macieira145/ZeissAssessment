namespace ZeissAssessment.Domain.Exceptions.Stock;

public sealed class InsufficientStockException : DomainException
{
    public int Available { get; }
    public int Requested { get; set; }

    public InsufficientStockException(int available, int requested) : base(
        $"Cannot decrement {requested} from stock of {available}.")
    {
        Available = available;
        Requested = requested;
    }
}