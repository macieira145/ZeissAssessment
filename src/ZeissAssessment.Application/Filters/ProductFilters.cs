namespace ZeissAssessment.Application.Filters;

public class ProductSearchFilter
{
    public string? Name { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinPrice { get; set; }
}