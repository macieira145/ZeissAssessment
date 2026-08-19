using Microsoft.EntityFrameworkCore;
using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.HasSequence<int>("ProductIdSequence", schema: "dbo")
            .StartsAt(1)
            .IncrementsBy(1);
        
        base.OnModelCreating(modelBuilder);
    }
}