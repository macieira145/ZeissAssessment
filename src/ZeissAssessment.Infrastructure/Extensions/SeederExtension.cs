using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZeissAssessment.Infrastructure.Persistence;
using ZeissAssessment.Infrastructure.Persistence.Seeders;

namespace ZeissAssessment.Infrastructure.Extensions;

public static class SeederExtension
{
    /// <summary>
    /// Applies pending EF Core migrations. Intended to be run as an explicit, standalone
    /// step (e.g. a CI/CD release step or init job) rather than on every app boot.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();
    }

    public static async Task SeedDevelopmentDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await ProductSeeder.SeedAsync(db);
    }
}
