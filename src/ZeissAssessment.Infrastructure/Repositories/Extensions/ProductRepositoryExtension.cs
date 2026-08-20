using ZeissAssessment.Application.Filters;
using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Infrastructure.Repositories.Extensions;

public static class ProductRepositoryExtension
{
    public static IQueryable<Product> AddSearchFilters(this IQueryable<Product> query, ProductSearchFilter filter)
    {
        if (filter.Name is not null)
        {
            query = query.Where(p => p.Name.ToLower().Contains(filter.Name));
        }

        if (filter.MinPrice is not null)
        {
            query = query.Where(p => p.Price >= filter.MinPrice);
        }

        if (filter.MaxPrice is not null)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice);
        }

        return query;
    }

    public static IQueryable<Product> AddStockLevelFilters(this IQueryable<Product> query,
        ProductStockLevelFilter filter)
    {
        query = query.Where(p => p.Stock.Quantity >= filter.MinStock && p.Stock.Quantity <= filter.MaxStock);

        return query;
    }
}