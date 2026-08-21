using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZeissAssessment.Domain.Entities;
using ZeissAssessment.Infrastructure.Persistence;

namespace ZeissAssessment.IntegrationTests.Fixtures.Products;

/// <summary>
/// Base class for controller integration tests. Isolation strategy: every test starts
/// from an empty Products table (DELETE in [SetUp]) rather than a rolled-back transaction,
/// since each HTTP request through the WebApplicationFactory resolves its own scoped
/// DbContext/connection that a test-owned transaction can't span.
/// </summary>
public abstract class ProductsApiTestBase
{
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public async Task BaseSetUpAsync()
    {
        Client = IntegrationTestFixture.Factory.CreateClient();
        await ClearProductsTableAsync();
    }

    [TearDown]
    public void BaseTearDown()
    {
        Client.Dispose();
    }

    protected static async Task ClearProductsTableAsync()
    {
        using var scope = IntegrationTestFixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.Products");
    }

    protected static async Task<Product> SeedProductAsync(Product product)
    {
        using var scope = IntegrationTestFixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
        return product;
    }

    protected static async Task<Product?> FindProductAsync(int id)
    {
        using var scope = IntegrationTestFixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }
}
