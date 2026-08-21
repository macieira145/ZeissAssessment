using Microsoft.EntityFrameworkCore;
using ZeissAssessment.Application.Filters;
using ZeissAssessment.Application.Interfaces.Repositories;
using ZeissAssessment.Domain.Entities;
using ZeissAssessment.Infrastructure.Persistence;
using ZeissAssessment.Infrastructure.Repositories.Extensions;

namespace ZeissAssessment.Infrastructure.Repositories;

public class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken)
    {
        var productEntry = await dbContext.Products.AddAsync(product, cancellationToken);

        return productEntry.Entity;
    }

    public async Task<Product?> GetByIdAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        return product;
    }

    public async Task<ICollection<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await dbContext.Products.AsNoTracking().ToListAsync(cancellationToken);

        return products;
    }

    public void Remove(Product product, CancellationToken cancellationToken)
    {
        dbContext.Remove(product);
    }

    public async Task<ICollection<Product>> Search(ProductSearchFilter filter, CancellationToken cancellationToken)
    {
        var products = await dbContext.Products.AsNoTracking().AddSearchFilters(filter).ToListAsync(cancellationToken);

        return products;
    }

    public async Task<ICollection<Product>> StockLevelSearch(ProductStockLevelFilter filter,
        CancellationToken cancellationToken)
    {
        var products = await dbContext.Products.AsNoTracking().AddStockLevelFilters(filter).ToListAsync(cancellationToken);

        return products;
    }
}