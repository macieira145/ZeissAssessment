using ZeissAssessment.Domain.ValueObjects;

namespace ZeissAssessment.Domain.Entities;

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required double Price { get; set; }
    public required Stock Stock { get; set; }
}