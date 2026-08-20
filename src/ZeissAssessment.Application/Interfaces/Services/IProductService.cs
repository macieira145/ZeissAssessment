using ZeissAssessment.Application.Contracts.Products;

namespace ZeissAssessment.Application.Interfaces.Services;

public interface IProductService
{
    public Task<ProductResponse> GetByIdAsync(int productId, CancellationToken cancellationToken);
    public Task<IEnumerable<ProductResponse>> GetAllAsync(CancellationToken cancellationToken);
    public Task<ProductResponse> CreateAsync(CreateProductRequest product, CancellationToken cancellationToken);

    public Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest productRequest,
        CancellationToken cancellationToken);

    public Task RemoveAsync(int productId, CancellationToken cancellationToken);
}