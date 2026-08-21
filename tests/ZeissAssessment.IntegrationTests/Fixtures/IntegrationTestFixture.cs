using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using ZeissAssessment.Infrastructure.Persistence;

namespace ZeissAssessment.IntegrationTests;

/// <summary>
/// Starts one SQL Server Testcontainer and one WebApplicationFactory for the entire
/// ZeissAssessment.IntegrationTests assembly. Applies to every test in this namespace
/// </summary>
[SetUpFixture]
public class IntegrationTestFixture
{
    private static MsSqlContainer? _container;
    private static WebApplicationFactory<Program>? _factory;

    public static WebApplicationFactory<Program> Factory =>
        _factory ?? throw new InvalidOperationException(
            "The integration test fixture has not been initialized yet.");

    [OneTimeSetUp]
    public async Task RunBeforeAnyTestsAsync()
    {
        _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest").Build();
        await _container.StartAsync();

        // "Testing" (not "Development") so ProductSeeder's fake data never loads - every
        // test arranges its own deterministic data.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _container.GetConnectionString());

        _factory = new WebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTestsAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
