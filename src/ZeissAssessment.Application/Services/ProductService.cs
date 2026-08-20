using ZeissAssessment.Application.Contracts.Products;
using ZeissAssessment.Application.Exceptions;
using ZeissAssessment.Application.Interfaces;
using ZeissAssessment.Application.Interfaces.Repositories;
using ZeissAssessment.Application.Interfaces.Services;
using ZeissAssessment.Application.Mappers;
using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Application.Services;

public class ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork, ProductMapper mapper)
    : IProductService
{
    public async Task<ProductResponse> GetByIdAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        return mapper.ToResponse(product);
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        return mapper.ToResponse(products);
    }

    public Task<ProductResponse> CreateAsync(Product product, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest productRequest,
        CancellationToken cancellationToken)
    {
        var product = mapper.ToEntity(productRequest);

        var createdProduct = await productRepository.CreateAsync(product, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = mapper.ToResponse(createdProduct);

        return response;
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest productRequest,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        mapper.UpdateEntity(productRequest, product);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = mapper.ToResponse(product);

        return response;
    }

    public async Task RemoveAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        productRepository.Remove(product, cancellationToken);

        var op = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (op <= 0)
        {
            throw PersistenceException.DeleteFailed(nameof(Product), productId);
        }
    }
}