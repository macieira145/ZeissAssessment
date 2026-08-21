using System.ComponentModel.DataAnnotations;
using Shouldly;
using ZeissAssessment.Application.Contracts.Products;

namespace ZeissAssessment.UnitTests.Application.Contracts.Products;

public class StockLevelProductsRequestTests
{
    [Test]
    public void Validate_ShouldReturnValidationErrors_WhenMinStockIsGreaterThanOrEqualToMaxStock()
    {
        // Arrange
        var request = new StockLevelProductsRequest { MinStock = 10, MaxStock = 10 };
        var context = new ValidationContext(request);

        // Act
        var results = request.Validate(context).ToList();

        // Assert
        results.ShouldSatisfyAllConditions(
            () => results.ShouldNotBeEmpty(),
            () => results.ShouldContain(r => r.MemberNames.Contains(nameof(StockLevelProductsRequest.MinStock))),
            () => results.ShouldContain(r => r.MemberNames.Contains(nameof(StockLevelProductsRequest.MaxStock))));
    }

    [Test]
    public void Validate_ShouldReturnNoValidationErrors_WhenMinStockIsLessThanMaxStock()
    {
        // Arrange
        var request = new StockLevelProductsRequest { MinStock = 5, MaxStock = 10 };
        var context = new ValidationContext(request);

        // Act
        var results = request.Validate(context).ToList();

        // Assert
        results.ShouldBeEmpty();
    }
}
