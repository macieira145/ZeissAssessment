using Microsoft.EntityFrameworkCore;
using ZeissAssessment.Application.Exceptions;
using ZeissAssessment.Application.Interfaces;

namespace ZeissAssessment.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entityName = ex.Entries.FirstOrDefault()?.Metadata.ClrType.Name ?? "Entity";
            var key = ex.Entries.FirstOrDefault()?.Property("Id").CurrentValue ?? "unknown";

            throw new ConcurrencyConflictException(entityName, key);
        }
    }

    public int SaveChanges()
    {
        try
        {
            return _dbContext.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entityName = ex.Entries.FirstOrDefault()?.Metadata.ClrType.Name ?? "Entity";
            var key = ex.Entries.FirstOrDefault()?.Property("Id").CurrentValue ?? "unknown";

            throw new ConcurrencyConflictException(entityName, key);
        }
    }

    public void DetachAllTrackedEntities()
    {
        _dbContext.ChangeTracker.Clear();
    }
}