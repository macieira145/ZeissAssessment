using Shouldly;
using ZeissAssessment.Application.Mappers;
using ZeissAssessment.TestCommon.Builders.Products;

namespace ZeissAssessment.UnitTests.Application.Mappers.Products;

public class ProductMapperTests
{
    private readonly ProductMapper _mapper = new();

    [Test]
    public void ToResponse_ShouldMapAllFieldsIncludingFlattenedStockQuantity_WhenGivenProduct()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithId(42)
            .WithName("Widget")
            .WithDescription("A widget.")
            .WithPrice(12.34m)
            .WithStock(7)
            .Build();

        // Act
        var response = _mapper.ToResponse(product);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.Id.ShouldBe(42),
            () => response.Name.ShouldBe("Widget"),
            () => response.Description.ShouldBe("A widget."),
            () => response.Price.ShouldBe(12.34m),
            () => response.Stock.ShouldBe(7),
            () => response.Created.ShouldBe(product.Created),
            () => response.Updated.ShouldBe(product.Updated));
    }

    [Test]
    public void ToResponse_ShouldMapEachProduct_WhenGivenCollection()
    {
        // Arrange
        var products = new[]
        {
            new ProductBuilder().WithId(1).WithName("First").Build(),
            new ProductBuilder().WithId(2).WithName("Second").Build()
        };

        // Act
        var responses = _mapper.ToResponse(products);

        // Assert
        responses.Select(r => r.Id).ShouldBe([1, 2]);
    }

    [Test]
    public void ToEntity_ShouldCreateProductWithZeroIdAndCreatedStock_WhenGivenCreateRequest()
    {
        // Arrange
        var request = new CreateProductRequestBuilder()
            .WithName("New Product")
            .WithDescription("A new product.")
            .WithPrice(5.50m)
            .WithStock(20)
            .Build();

        // Act
        var product = _mapper.ToEntity(request);

        // Assert
        product.ShouldSatisfyAllConditions(
            () => product.Id.ShouldBe(0),
            () => product.Name.ShouldBe("New Product"),
            () => product.Description.ShouldBe("A new product."),
            () => product.Price.ShouldBe(5.50m),
            () => product.Stock.Quantity.ShouldBe(20));
    }

    [Test]
    public void UpdateEntity_ShouldUpdateNameDescriptionAndPrice_WhenGivenUpdateRequest()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        var request = new UpdateProductRequestBuilder()
            .WithName("Updated Name")
            .WithDescription("Updated description.")
            .WithPrice(99.99m)
            .Build();

        // Act
        _mapper.UpdateEntity(request, product);

        // Assert
        product.ShouldSatisfyAllConditions(
            () => product.Name.ShouldBe("Updated Name"),
            () => product.Description.ShouldBe("Updated description."),
            () => product.Price.ShouldBe(99.99m));
    }

    [Test]
    public void UpdateEntity_ShouldNotChangeIdCreatedUpdatedOrStock_WhenGivenUpdateRequest()
    {
        // Arrange
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var product = new ProductBuilder()
            .WithId(7)
            .WithStock(50)
            .WithCreated(createdAt)
            .WithUpdated(updatedAt)
            .Build();
        var request = new UpdateProductRequestBuilder().Build();

        // Act
        _mapper.UpdateEntity(request, product);

        // Assert
        product.ShouldSatisfyAllConditions(
            () => product.Id.ShouldBe(7),
            () => product.Stock.Quantity.ShouldBe(50),
            () => product.Created.ShouldBe(createdAt),
            () => product.Updated.ShouldBe(updatedAt));
    }
}
