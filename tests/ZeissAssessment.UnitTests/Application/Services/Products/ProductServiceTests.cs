using Moq;
using Shouldly;
using ZeissAssessment.Application.Contracts.Products;
using ZeissAssessment.Application.Exceptions;
using ZeissAssessment.Application.Filters;
using ZeissAssessment.Application.Interfaces;
using ZeissAssessment.Application.Interfaces.Repositories;
using ZeissAssessment.Application.Mappers;
using ZeissAssessment.Application.Services;
using ZeissAssessment.Domain.Entities;
using ZeissAssessment.Domain.Exceptions.Stock;
using ZeissAssessment.TestCommon.Builders.Products;

namespace ZeissAssessment.UnitTests.Application.Services.Products;

public class ProductServiceTests
{
    private Mock<IProductRepository> _productRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private ProductService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _productRepository = new Mock<IProductRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _sut = new ProductService(_productRepository.Object, _unitOfWork.Object, new ProductMapper(), new ProductFilterMapper());
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnMappedProduct_WhenProductExists()
    {
        // Arrange
        var product = new ProductBuilder().WithId(1).WithName("Widget").Build();
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(product);

        // Act
        var response = await _sut.GetByIdAsync(1, CancellationToken.None);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.Id.ShouldBe(1),
            () => response.Name.ShouldBe("Widget"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync((Product?)null);

        // Act
        var exception = await Should.ThrowAsync<NotFoundException>(() => _sut.GetByIdAsync(1, CancellationToken.None));

        // Assert
        exception.Message.ShouldContain("1");
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllMappedProducts_WhenProductsExist()
    {
        // Arrange
        var products = new List<Product>
        {
            new ProductBuilder().WithId(1).Build(),
            new ProductBuilder().WithId(2).Build()
        };
        _productRepository.Setup(r => r.GetAllAsync(CancellationToken.None)).ReturnsAsync(products);

        // Act
        var responses = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        responses.Select(r => r.Id).ShouldBe([1, 2]);
    }

    [Test]
    public async Task CreateAsync_ShouldPersistAndReturnMappedProduct_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateProductRequestBuilder().WithName("New Product").Build();
        _productRepository
            .Setup(r => r.CreateAsync(It.Is<Product>(p => p.Name == "New Product"), CancellationToken.None))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        // Act
        var response = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        response.Name.ShouldBe("New Product");
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateAndReturnMappedProduct_WhenProductExists()
    {
        // Arrange
        var product = new ProductBuilder().WithId(1).WithName("Old Name").Build();
        var request = new UpdateProductRequestBuilder().WithName("New Name").Build();
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(product);

        // Act
        var response = await _sut.UpdateAsync(1, request, CancellationToken.None);

        // Assert
        response.Name.ShouldBe("New Name");
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var request = new UpdateProductRequestBuilder().Build();
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync((Product?)null);

        // Act
        await Should.ThrowAsync<NotFoundException>(() => _sut.UpdateAsync(1, request, CancellationToken.None));

        // Assert
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RemoveAsync_ShouldRemoveProductAndSave_WhenProductExists()
    {
        // Arrange
        var product = new ProductBuilder().WithId(1).Build();
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(product);
        _unitOfWork.Setup(u => u.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        // Act
        await _sut.RemoveAsync(1, CancellationToken.None);

        // Assert
        _productRepository.Verify(r => r.Remove(product, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task RemoveAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync((Product?)null);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => _sut.RemoveAsync(1, CancellationToken.None));
    }

    [Test]
    public async Task RemoveAsync_ShouldThrowPersistenceException_WhenSaveChangesAffectsNoRows()
    {
        // Arrange
        var product = new ProductBuilder().WithId(1).Build();
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(product);
        _unitOfWork.Setup(u => u.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(0);

        // Act
        var exception = await Should.ThrowAsync<PersistenceException>(() => _sut.RemoveAsync(1, CancellationToken.None));

        // Assert
        exception.Message.ShouldContain("1");
    }

    [Test]
    public async Task IncrementStock_ShouldIncreaseStockAndSave_WhenProductExists()
    {
        // Arrange
        var product = new ProductBuilder().WithId(1).WithStock(10).Build();
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(product);

        // Act
        var response = await _sut.IncrementStock(1, 5, CancellationToken.None);

        // Assert
        response.Stock.ShouldBe(15);
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task IncrementStock_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync((Product?)null);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => _sut.IncrementStock(1, 5, CancellationToken.None));
    }

    [Test]
    public async Task DecrementStock_ShouldDecreaseStockAndSave_WhenProductExists()
    {
        // Arrange
        var product = new ProductBuilder().WithId(1).WithStock(10).Build();
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(product);

        // Act
        var response = await _sut.DecrementStock(1, 4, CancellationToken.None);

        // Assert
        response.Stock.ShouldBe(6);
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task DecrementStock_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync((Product?)null);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => _sut.DecrementStock(1, 5, CancellationToken.None));
    }

    [Test]
    public async Task DecrementStock_ShouldThrowInsufficientStockExceptionAndNotSave_WhenQuantityExceedsStock()
    {
        // Arrange
        var product = new ProductBuilder().WithId(1).WithStock(5).Build();
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(product);

        // Act
        await Should.ThrowAsync<InsufficientStockException>(() => _sut.DecrementStock(1, 10, CancellationToken.None));

        // Assert
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DecrementStock_ShouldRetryAndSucceed_WhenConcurrencyConflictOccursOnce()
    {
        // Arrange
        var product = new ProductBuilder().WithId(1).WithStock(10).Build();
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(product);
        _unitOfWork.SetupSequence(u => u.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(new ConcurrencyConflictException(nameof(Product), 1))
            .ReturnsAsync(1);

        // Act
        var response = await _sut.DecrementStock(1, 4, CancellationToken.None);

        // Assert: GetOrThrowAsync is re-invoked after the retry and re-applies the mutation to the same
        // mocked product instance (10 -> 6 on the failed attempt, then 6 -> 2 on the retried attempt).
        response.Stock.ShouldBe(2);
        _unitOfWork.Verify(u => u.DetachAllTrackedEntities(), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Exactly(2));
    }

    [Test]
    public async Task DecrementStock_ShouldThrowConcurrencyConflictException_WhenRetriesExhausted()
    {
        // Arrange
        var product = new ProductBuilder().WithId(1).WithStock(100).Build();
        _productRepository.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(product);
        _unitOfWork.Setup(u => u.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(new ConcurrencyConflictException(nameof(Product), 1));

        // Act & Assert
        await Should.ThrowAsync<ConcurrencyConflictException>(() =>
            _sut.DecrementStock(1, 4, CancellationToken.None));
    }

    [Test]
    public async Task Search_ShouldReturnMappedFilteredProducts_WhenRequestHasFilters()
    {
        // Arrange
        var request = new SearchProductsRequest { Name = "widget", MinPrice = 1m, MaxPrice = 100m };
        var products = new List<Product> { new ProductBuilder().WithId(1).WithName("Widget").Build() };
        _productRepository
            .Setup(r => r.Search(It.Is<ProductSearchFilter>(f => f.Name == "widget" && f.MinPrice == 1m && f.MaxPrice == 100m),
                CancellationToken.None))
            .ReturnsAsync(products);

        // Act
        var responses = await _sut.Search(request, CancellationToken.None);

        // Assert
        responses.ShouldHaveSingleItem().Name.ShouldBe("Widget");
    }

    [Test]
    public async Task StockLevelSearch_ShouldReturnMappedFilteredProducts_WhenRequestHasStockRange()
    {
        // Arrange
        var request = new StockLevelProductsRequest { MinStock = 5, MaxStock = 50 };
        var products = new List<Product> { new ProductBuilder().WithId(1).WithStock(20).Build() };
        _productRepository
            .Setup(r => r.StockLevelSearch(It.Is<ProductStockLevelFilter>(f => f.MinStock == 5 && f.MaxStock == 50),
                CancellationToken.None))
            .ReturnsAsync(products);

        // Act
        var responses = await _sut.StockLevelSearch(request, CancellationToken.None);

        // Assert
        responses.ShouldHaveSingleItem().Stock.ShouldBe(20);
    }
}
