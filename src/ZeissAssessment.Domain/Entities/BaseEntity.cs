namespace ZeissAssessment.Domain.Entities;

public class BaseEntity
{
    public required int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}