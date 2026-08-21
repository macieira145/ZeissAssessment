using Shouldly;
using ZeissAssessment.Application.Contracts.Products;
using ZeissAssessment.Application.Mappers;

namespace ZeissAssessment.UnitTests.Application.Mappers.Products;

public class ProductFilterMapperTests
{
    private readonly ProductFilterMapper _mapper = new();

    [Test]
    public void ToFilter_ShouldMapAllProperties_WhenGivenSearchProductsRequest()
    {
        // Arrange
        var request = new SearchProductsRequest
        {
            Name = "widget",
            MinPrice = 1.00m,
            MaxPrice = 100.00m
        };

        // Act
        var filter = _mapper.ToFilter(request);

        // Assert
        filter.ShouldSatisfyAllConditions(
            () => filter.Name.ShouldBe("widget"),
            () => filter.MinPrice.ShouldBe(1.00m),
            () => filter.MaxPrice.ShouldBe(100.00m));
    }

    [Test]
    public void ToFilter_ShouldMapAllProperties_WhenGivenStockLevelProductsRequest()
    {
        // Arrange
        var request = new StockLevelProductsRequest
        {
            MinStock = 5,
            MaxStock = 50
        };

        // Act
        var filter = _mapper.ToFilter(request);

        // Assert
        filter.ShouldSatisfyAllConditions(
            () => filter.MinStock.ShouldBe(5),
            () => filter.MaxStock.ShouldBe(50));
    }
}
