using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZeissAssessment.Infrastructure.Persistence;
using ZeissAssessment.Infrastructure.Persistence.Seeders;

namespace ZeissAssessment.Infrastructure.Extensions;

public static class SeederExtension
{
    public static async Task ApplyMigrationsAndSeedAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        if (scope.ServiceProvider.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            await ProductSeeder.SeedAsync(db);
        }
    }
}