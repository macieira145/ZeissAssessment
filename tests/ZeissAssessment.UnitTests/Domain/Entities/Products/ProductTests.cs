using Shouldly;
using ZeissAssessment.Domain.Exceptions.Stock;
using ZeissAssessment.TestCommon.Builders.Products;

namespace ZeissAssessment.UnitTests.Domain.Entities.Products;

public class ProductTests
{
    [Test]
    public void IncrementStock_ShouldUpdateStockQuantity_WhenAmountIsValid()
    {
        // Arrange
        var product = new ProductBuilder().WithStock(10).Build();

        // Act
        product.IncrementStock(5);

        // Assert
        product.Stock.Quantity.ShouldBe(15);
    }

    [Test]
    public void DecrementStock_ShouldUpdateStockQuantity_WhenAmountIsValid()
    {
        // Arrange
        var product = new ProductBuilder().WithStock(10).Build();

        // Act
        product.DecrementStock(4);

        // Assert
        product.Stock.Quantity.ShouldBe(6);
    }

    [Test]
    public void DecrementStock_ShouldThrowInsufficientStockException_WhenAmountExceedsAvailableStock()
    {
        // Arrange
        var product = new ProductBuilder().WithStock(10).Build();

        // Act
        Should.Throw<InsufficientStockException>(() => product.DecrementStock(11));

        // Assert
        product.Stock.Quantity.ShouldBe(10);
    }
}
