namespace ZeissAssessment.Application.Filters;

public class ProductSearchFilter
{
    public string? Name { get; set; }
    public double? MaxPrice { get; set; }
    public double? MinPrice { get; set; }
}