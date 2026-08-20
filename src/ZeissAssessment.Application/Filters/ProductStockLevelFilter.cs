namespace ZeissAssessment.Application.Filters;

public class ProductStockLevelFilter
{
    public int? MinStock { get; set; }
    public int? MaxStock { get; set; }
}