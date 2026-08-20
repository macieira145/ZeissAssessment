using Microsoft.EntityFrameworkCore;
using ZeissAssessment.Infrastructure.Persistence.Seeders.Fakers;

namespace ZeissAssessment.Infrastructure.Persistence.Seeders;

public class ProductSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext, int count = 80)
    {
        if (await dbContext.Products.AnyAsync())
        {
            return;
        }
        
        var products = ProductFaker.Create().Generate(count);
        
        await dbContext.Products.AddRangeAsync(products);
        await dbContext.SaveChangesAsync();
    }
}