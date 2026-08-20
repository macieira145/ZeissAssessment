using System.Runtime.CompilerServices;
using Bogus;
using ZeissAssessment.Domain.Entities;
using ZeissAssessment.Domain.ValueObjects;

namespace ZeissAssessment.Infrastructure.Persistence.Seeders.Fakers;

public static class ProductFaker
{
    public static Faker<Product> Create()
    {
        var faker = new Faker<Product>()
            .CustomInstantiator(f => new Product()
            {
                Id = 0,
                Name = f.Commerce.ProductName(),
                Price = decimal.Parse(f.Commerce.Price(5, 500)),
                Stock = Stock.Create(f.Random.Int(0, 500)),
                Description = f.Commerce.ProductDescription(),
                Created = DateTime.Now,
                Updated = DateTime.Now,
            });

        return faker;
    }
}