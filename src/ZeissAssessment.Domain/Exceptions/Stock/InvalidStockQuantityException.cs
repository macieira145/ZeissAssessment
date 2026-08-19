namespace ZeissAssessment.Domain.Exceptions.Stock;

public class InvalidStockQuantityException : DomainException
{
    public InvalidStockQuantityException(int quantity) : base($"Stock quantity cannot be negative. Received: {quantity}") { }
}