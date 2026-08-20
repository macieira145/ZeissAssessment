namespace ZeissAssessment.Application.Contracts.Products;

public class SearchProductsRequest
{
    public string? Name { get; set; }
    public double? MaxPrice { get; set; }
    public double? MinPrice { get; set; }
}