using ZeissAssessment.Application.Contracts.Products;
using ZeissAssessment.Application.Exceptions;
using ZeissAssessment.Application.Interfaces;
using ZeissAssessment.Application.Interfaces.Repositories;
using ZeissAssessment.Application.Interfaces.Services;
using ZeissAssessment.Application.Mappers;
using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Application.Services;

public class ProductService(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ProductMapper productMapper,
    ProductFilterMapper productFilterMapper)
    : IProductService
{
    private const int MaxConcurrencyRetries = 3;


    public async Task<ProductResponse> GetByIdAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await GetOrThrowAsync(productId, cancellationToken);

        return productMapper.ToResponse(product);
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        return productMapper.ToResponse(products);
    }

    public Task<ProductResponse> CreateAsync(Product product, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest productRequest,
        CancellationToken cancellationToken)
    {
        var product = productMapper.ToEntity(productRequest);

        var createdProduct = await productRepository.CreateAsync(product, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = productMapper.ToResponse(createdProduct);

        return response;
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest productRequest,
        CancellationToken cancellationToken)
    {
        var product = await GetOrThrowAsync(id, cancellationToken);

        productMapper.UpdateEntity(productRequest, product);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = productMapper.ToResponse(product);

        return response;
    }

    public async Task RemoveAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await GetOrThrowAsync(productId, cancellationToken);

        productRepository.Remove(product, cancellationToken);

        var op = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (op <= 0)
        {
            throw PersistenceException.DeleteFailed(nameof(Product), productId);
        }
    }

    public async Task<ProductResponse> IncrementStock(int productId, int stock, CancellationToken cancellationToken)
    {
        var product = await ExecuteWithConcurrencyRetryAsync(
            productId,
            p => p.IncrementStock(stock),
            cancellationToken);

        return productMapper.ToResponse(product);
    }

    public async Task<ProductResponse> DecrementStock(int productId, int stock, CancellationToken cancellationToken)
    {
        var product = await ExecuteWithConcurrencyRetryAsync(
            productId,
            p => p.DecrementStock(stock),
            cancellationToken);

        return productMapper.ToResponse(product);
    }

    private async Task<Product> ExecuteWithConcurrencyRetryAsync(int productId, Action<Product> mutate,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            var product = await GetOrThrowAsync(productId, cancellationToken);

            mutate(product);

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return product;
            }
            catch (ConcurrencyConflictException) when (attempt < MaxConcurrencyRetries)
            {
                unitOfWork.DetachAllTrackedEntities();
            }
        }
    }

    public async Task<IEnumerable<ProductResponse>> Search(SearchProductsRequest request,
        CancellationToken cancellationToken)
    {
        var filter = productFilterMapper.ToFilter(request);

        var products = await productRepository.Search(filter, cancellationToken);

        var response = productMapper.ToResponse(products);

        return response;
    }

    public async Task<IEnumerable<ProductResponse>> StockLevelSearch(StockLevelProductsRequest request,
        CancellationToken cancellationToken)
    {
        var filter = productFilterMapper.ToFilter(request);

        var products = await productRepository.StockLevelSearch(filter, cancellationToken);

        var response = productMapper.ToResponse(products);

        return response;
    }

    private async Task<Product> GetOrThrowAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        return product;
    }
}