using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using ZeissAssessment.Application.Contracts.Products;
using ZeissAssessment.IntegrationTests.Fixtures.Products;
using ZeissAssessment.TestCommon.Builders.Products;

namespace ZeissAssessment.IntegrationTests.Controllers.Products;

public class ProductsControllerTests : ProductsApiTestBase
{
    #region CRUD 
    
    [Test]
    public async Task Create_ShouldPersistProductAndReturnIt_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateProductRequestBuilder()
            .WithName("Integration Widget")
            .WithDescription("Created via integration test.")
            .WithPrice(15.50m)
            .WithStock(25)
            .Build();

        // Act
        var httpResponse = await Client.PostAsJsonAsync("/api/products", request);

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await httpResponse.Content.ReadFromJsonAsync<ProductResponse>();
        body.ShouldNotBeNull();

        var persisted = await FindProductAsync(body!.Id);
        persisted.ShouldSatisfyAllConditions(
            () => persisted.ShouldNotBeNull(),
            () => persisted!.Id.ShouldBeInRange(100_000, 999_999),
            () => persisted!.Name.ShouldBe("Integration Widget"),
            () => persisted!.Description.ShouldBe("Created via integration test."),
            () => persisted!.Price.ShouldBe(15.50m),
            () => persisted!.Stock.Quantity.ShouldBe(25));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequestWithValidationErrors_WhenNameIsMissing()
    {
        // Arrange
        var payload = new
        {
            Name = "",
            Description = "A perfectly valid description.",
            Price = 10.00m,
            Stock = 5
        };

        // Act
        var httpResponse = await Client.PostAsJsonAsync("/api/products", payload);

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.ShouldSatisfyAllConditions(
            () => problem.ShouldNotBeNull(),
            () => problem!.Extensions.ShouldContainKey("errors"));
    }

    [Test]
    public async Task Create_ShouldRoundPriceToConfiguredPrecision_WhenPriceHasMoreThanTwoDecimalPlaces()
    {
        // Arrange
        var request = new CreateProductRequestBuilder().WithPrice(19.999m).Build();

        // Act
        var httpResponse = await Client.PostAsJsonAsync("/api/products", request);

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await httpResponse.Content.ReadFromJsonAsync<ProductResponse>();
        var persisted = await FindProductAsync(body!.Id);
        persisted!.Price.ShouldBe(20.00m);
    }

    [Test]
    public async Task GetById_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var product = await SeedProductAsync(new ProductBuilder().WithName("Existing Product").Build());

        // Act
        var httpResponse = await Client.GetAsync($"/api/products/{product.Id}");

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await httpResponse.Content.ReadFromJsonAsync<ProductResponse>();
        body!.Name.ShouldBe("Existing Product");
    }

    [Test]
    public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Act
        var httpResponse = await Client.GetAsync("/api/products/999999");

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetAll_ShouldReturnAllPersistedProducts()
    {
        // Arrange
        await SeedProductAsync(new ProductBuilder().WithName("Product A").Build());
        await SeedProductAsync(new ProductBuilder().WithName("Product B").Build());

        // Act
        var httpResponse = await Client.GetAsync("/api/products");

        // Assert
        var body = await httpResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        body!.Select(p => p.Name).ShouldBe(["Product A", "Product B"], ignoreOrder: true);
    }

    [Test]
    public async Task Update_ShouldPersistChangedFieldsAndPreserveStockAndId_WhenRequestIsValid()
    {
        // Arrange
        var product = await SeedProductAsync(new ProductBuilder().WithName("Before Update").WithStock(30).Build());
        var request = new UpdateProductRequestBuilder().WithName("After Update").WithPrice(50.00m).Build();

        // Act
        var httpResponse = await Client.PutAsJsonAsync($"/api/products/{product.Id}", request);

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var persisted = await FindProductAsync(product.Id);
        persisted.ShouldSatisfyAllConditions(
            () => persisted!.Id.ShouldBe(product.Id),
            () => persisted!.Name.ShouldBe("After Update"),
            () => persisted!.Price.ShouldBe(50.00m),
            () => persisted!.Stock.Quantity.ShouldBe(30));
    }

    [Test]
    public async Task Update_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var request = new UpdateProductRequestBuilder().Build();

        // Act
        var httpResponse = await Client.PutAsJsonAsync("/api/products/999999", request);

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Delete_ShouldRemoveProduct_WhenProductExists()
    {
        // Arrange
        var product = await SeedProductAsync(new ProductBuilder().Build());

        // Act
        var httpResponse = await Client.DeleteAsync($"/api/products/{product.Id}");

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var persisted = await FindProductAsync(product.Id);
        persisted.ShouldBeNull();
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Act
        var httpResponse = await Client.DeleteAsync("/api/products/999999");

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    #endregion
    
    #region Stock

    [Test]
    public async Task AddToStock_ShouldIncreaseStockAndPersist_WhenProductExists()
    {
        // Arrange
        var product = await SeedProductAsync(new ProductBuilder().WithStock(10).Build());

        // Act
        var httpResponse = await Client.PostAsync($"/api/products/{product.Id}/add-to-stock/5", null);

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var persisted = await FindProductAsync(product.Id);
        persisted!.Stock.Quantity.ShouldBe(15);
    }

    [Test]
    public async Task DecrementStock_ShouldDecreaseStockAndPersist_WhenQuantityIsAvailable()
    {
        // Arrange
        var product = await SeedProductAsync(new ProductBuilder().WithStock(10).Build());

        // Act
        var httpResponse = await Client.PostAsync($"/api/products/{product.Id}/decrement-stock/4", null);

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var persisted = await FindProductAsync(product.Id);
        persisted!.Stock.Quantity.ShouldBe(6);
    }

    [Test]
    public async Task DecrementStock_ShouldReturnBadRequestAndLeaveStockUnchanged_WhenQuantityExceedsAvailableStock()
    {
        // Arrange
        var product = await SeedProductAsync(new ProductBuilder().WithStock(5).Build());

        // Act
        var httpResponse = await Client.PostAsync($"/api/products/{product.Id}/decrement-stock/10", null);

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var persisted = await FindProductAsync(product.Id);
        persisted!.Stock.Quantity.ShouldBe(5);
    }

    #endregion
    
    #region Search

    [Test]
    public async Task Search_ShouldReturnCaseInsensitiveNameMatches_WhenNameFilterHasDifferentCasing()
    {
        // Arrange
        await SeedProductAsync(new ProductBuilder().WithName("Precision Widget").Build());
        await SeedProductAsync(new ProductBuilder().WithName("Unrelated Gadget").Build());

        // Act
        var httpResponse = await Client.GetAsync("/api/products/search?Name=WIDGET");

        // Assert
        var results = await httpResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        results!.ShouldHaveSingleItem().Name.ShouldBe("Precision Widget");
    }

    [Test]
    public async Task Search_ShouldReturnProductsWithinPriceRange_WhenMinAndMaxPriceProvided()
    {
        // Arrange
        await SeedProductAsync(new ProductBuilder().WithName("Cheap").WithPrice(5.00m).Build());
        await SeedProductAsync(new ProductBuilder().WithName("MidRange").WithPrice(50.00m).Build());
        await SeedProductAsync(new ProductBuilder().WithName("Expensive").WithPrice(500.00m).Build());

        // Act
        var httpResponse = await Client.GetAsync("/api/products/search?MinPrice=10&MaxPrice=100");

        // Assert
        var results = await httpResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        results!.ShouldHaveSingleItem().Name.ShouldBe("MidRange");
    }

    [Test]
    public async Task StockLevelSearch_ShouldReturnProductsWithinInclusiveStockRange_WhenMinAndMaxStockProvided()
    {
        // Arrange
        await SeedProductAsync(new ProductBuilder().WithName("Low").WithStock(4).Build());
        await SeedProductAsync(new ProductBuilder().WithName("AtMin").WithStock(5).Build());
        await SeedProductAsync(new ProductBuilder().WithName("AtMax").WithStock(20).Build());
        await SeedProductAsync(new ProductBuilder().WithName("High").WithStock(21).Build());

        // Act
        var httpResponse = await Client.GetAsync("/api/products/stock-level?MinStock=5&MaxStock=20");

        // Assert
        var results = await httpResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        results!.Select(r => r.Name).ShouldBe(["AtMin", "AtMax"], ignoreOrder: true);
    }

    [Test]
    public async Task StockLevelSearch_ShouldReturnBadRequest_WhenMinStockIsGreaterThanMaxStock()
    {
        // Act
        var httpResponse = await Client.GetAsync("/api/products/stock-level?MinStock=20&MaxStock=5");

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    #endregion
}