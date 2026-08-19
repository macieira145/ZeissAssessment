namespace ZeissAssessment.Domain.Entities;

public class BaseEntity
{
    public required int Id { get; set; }
    public DateTime Created { get; init; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;
}