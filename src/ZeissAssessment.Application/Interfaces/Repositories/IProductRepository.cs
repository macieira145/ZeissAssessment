using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Application.Interfaces.Repositories;

public interface IProductRepository
{
    public Task<Product> CreateAsync(Product product, CancellationToken cancellationToken);
    public Task<Product?> GetByIdAsync(int productId, CancellationToken cancellationToken);
    public Task<ICollection<Product>> GetAllAsync(CancellationToken cancellationToken);
    public void Remove(Product product, CancellationToken cancellationToken);
}