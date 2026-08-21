using Shouldly;
using ZeissAssessment.Domain.Exceptions.Stock;
using ZeissAssessment.Domain.ValueObjects;

namespace ZeissAssessment.UnitTests.Domain.ValueObjects.Products;

public class StockTests
{
    [Test]
    public void Create_ShouldCreateStockWithZeroQuantity_WhenQuantityIsZero()
    {
        // Arrange
        const int quantity = 0;

        // Act
        var stock = Stock.Create(quantity);

        // Assert
        stock.Quantity.ShouldBe(0);
    }

    [Test]
    public void Create_ShouldThrowInvalidStockQuantityException_WhenQuantityIsNegative()
    {
        // Arrange
        const int quantity = -1;

        // Act
        var exception = Should.Throw<InvalidStockQuantityException>(() => Stock.Create(quantity));

        // Assert
        exception.Message.ShouldContain("-1");
    }

    [Test]
    public void Increment_ShouldIncreaseQuantity_WhenAmountIsPositive()
    {
        // Arrange
        var stock = Stock.Create(10);

        // Act
        var result = stock.Increment(5);

        // Assert
        result.Quantity.ShouldBe(15);
    }

    [Test]
    public void Increment_ShouldThrowInvalidStockQuantityException_WhenAmountIsNegative()
    {
        // Arrange
        var stock = Stock.Create(10);

        // Act
        var exception = Should.Throw<InvalidStockQuantityException>(() => stock.Increment(-5));

        // Assert
        exception.Message.ShouldContain("-5");
    }

    [Test]
    public void Decrement_ShouldDecreaseQuantity_WhenAmountIsWithinAvailableStock()
    {
        // Arrange
        var stock = Stock.Create(10);

        // Act
        var result = stock.Decrement(4);

        // Assert
        result.Quantity.ShouldBe(6);
    }

    [Test]
    public void Decrement_ShouldReturnZeroStock_WhenAmountEqualsAvailableStock()
    {
        // Arrange
        var stock = Stock.Create(10);

        // Act
        var result = stock.Decrement(10);

        // Assert
        result.Quantity.ShouldBe(0);
    }

    [Test]
    public void Decrement_ShouldThrowInsufficientStockException_WhenAmountExceedsAvailableStock()
    {
        // Arrange
        var stock = Stock.Create(10);

        // Act
        var exception = Should.Throw<InsufficientStockException>(() => stock.Decrement(11));

        // Assert
        exception.ShouldSatisfyAllConditions(
            () => exception.Available.ShouldBe(10),
            () => exception.Requested.ShouldBe(11));
    }

    [Test]
    public void Decrement_ShouldThrowInvalidStockQuantityException_WhenAmountIsNegative()
    {
        // Arrange
        var stock = Stock.Create(10);

        // Act
        var exception = Should.Throw<InvalidStockQuantityException>(() => stock.Decrement(-1));

        // Assert
        exception.Message.ShouldContain("-1");
    }

    [Test]
    public void Equality_ShouldBeEqual_WhenQuantitiesMatch()
    {
        // Arrange
        var first = Stock.Create(5);
        var second = Stock.Create(5);

        // Act
        var areEqual = first == second;

        // Assert
        areEqual.ShouldSatisfyAllConditions(
            () => (first == second).ShouldBeTrue(),
            () => first.Equals(second).ShouldBeTrue(),
            () => (first != Stock.Create(6)).ShouldBeTrue());
    }
}
