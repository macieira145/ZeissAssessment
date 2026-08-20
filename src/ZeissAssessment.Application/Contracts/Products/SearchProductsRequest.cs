namespace ZeissAssessment.Application.Contracts.Products;

public class SearchProductsRequest
{
    public string? Name { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinPrice { get; set; }
}