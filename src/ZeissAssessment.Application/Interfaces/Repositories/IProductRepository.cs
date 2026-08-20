using ZeissAssessment.Application.Contracts.Products;
using ZeissAssessment.Application.Filters;
using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Application.Interfaces.Repositories;

public interface IProductRepository
{
    public Task<Product> CreateAsync(Product product, CancellationToken cancellationToken);
    public Task<Product?> GetByIdAsync(int productId, CancellationToken cancellationToken);
    public Task<ICollection<Product>> GetAllAsync(CancellationToken cancellationToken);
    public void Remove(Product product, CancellationToken cancellationToken);

    public Task<ICollection<Product>> Search(ProductSearchFilter filter,
        CancellationToken cancellationToken);
    
    public Task<ICollection<Product>> StockLevelSearch(ProductStockLevelFilter filter,
        CancellationToken cancellationToken);
}