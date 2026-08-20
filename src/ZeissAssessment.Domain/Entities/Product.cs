using ZeissAssessment.Domain.ValueObjects;

namespace ZeissAssessment.Domain.Entities;

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required double Price { get; set; }
    public required Stock Stock { get; set; }

    public void IncrementStock(int quantity)
    {
        Stock = Stock.Increment(quantity);
    }

    public void DecrementStock(int quantity)
    {
        Stock = Stock.Decrement(quantity);
    }
}