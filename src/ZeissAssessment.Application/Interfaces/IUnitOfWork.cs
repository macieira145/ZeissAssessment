namespace ZeissAssessment.Application.Interfaces;

public interface IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    public int SaveChanges();
}