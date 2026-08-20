namespace ZeissAssessment.Application.Contracts.Products;

public class ProductResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required double Price { get; init; }
    public required int Stock { get; init; }
    public DateTime Created { get; init; }
    public DateTime Updated { get; init; }
}