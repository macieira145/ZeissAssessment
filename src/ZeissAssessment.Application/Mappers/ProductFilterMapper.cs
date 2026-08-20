using Riok.Mapperly.Abstractions;
using ZeissAssessment.Application.Contracts.Products;
using ZeissAssessment.Application.Filters;

namespace ZeissAssessment.Application.Mappers;

[Mapper]
public partial class ProductFilterMapper
{
    public partial ProductSearchFilter ToFilter(SearchProductsRequest request);
    public partial ProductStockLevelFilter ToFilter(StockLevelProductsRequest request);
}