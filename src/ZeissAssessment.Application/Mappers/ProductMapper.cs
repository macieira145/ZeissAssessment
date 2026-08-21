using Riok.Mapperly.Abstractions;
using ZeissAssessment.Application.Contracts.Products;
using ZeissAssessment.Domain.Entities;
using ZeissAssessment.Domain.ValueObjects;

namespace ZeissAssessment.Application.Mappers;

[Mapper]
public partial class ProductMapper
{
    [MapProperty($"{nameof(Product.Stock)}.{nameof(Stock.Quantity)}", nameof(ProductResponse.Stock))]
    [MapperIgnoreSource(nameof(Product.RowVersion))]
    public partial ProductResponse ToResponse(Product product);

    public partial ICollection<ProductResponse> ToResponse(ICollection<Product> products);
    
    public Product ToEntity(CreateProductRequest request)
    {
        var product = new Product
        {
            Id = 0,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = Domain.ValueObjects.Stock.Create(request.Stock)
        };

        return product;
    }

    [MapperIgnoreTarget(nameof(Product.Id))]
    [MapperIgnoreTarget(nameof(Product.Created))]
    [MapperIgnoreTarget(nameof(Product.Updated))]
    [MapperIgnoreTarget(nameof(Product.Stock))]
    [MapperIgnoreTarget(nameof(Product.RowVersion))]
    public partial void UpdateEntity(UpdateProductRequest request, Product target);
}