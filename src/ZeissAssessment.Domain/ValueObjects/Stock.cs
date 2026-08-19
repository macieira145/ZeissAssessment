using ZeissAssessment.Domain.Exceptions.Stock;

namespace ZeissAssessment.Domain.ValueObjects;

public sealed class Stock : IEquatable<Stock>
{
    public int Quantity { get; }

    private Stock(int quantity)
    {
        Quantity = quantity;
    }

    public static Stock Create(int quantity)
    {
        if (quantity < 0)
        {
            throw new InvalidStockQuantityException(quantity);
        }

        return new Stock(quantity);
    }

    public Stock Add(int amount)
    {
        if (amount < 0)
        {
            throw new InvalidStockQuantityException(amount);
        }

        return new Stock(Quantity + amount);
    }

    public Stock Decrement(int amount)
    {
        if (amount < 0)
        {
            throw new InvalidStockQuantityException(amount);
        }

        if (amount > Quantity)
        {
            throw new InsufficientStockException(Quantity, amount);
        }

        return new Stock(Quantity - amount);
    }

    public bool Equals(Stock? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Quantity == other.Quantity;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Stock other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Quantity.GetHashCode();
    }

    public static bool operator ==(Stock? left, Stock? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Stock? left, Stock? right) => !(left == right);
}