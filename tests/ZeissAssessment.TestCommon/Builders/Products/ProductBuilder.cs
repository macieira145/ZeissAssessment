using ZeissAssessment.Domain.Entities;
using ZeissAssessment.Domain.ValueObjects;

namespace ZeissAssessment.TestCommon.Builders.Products;

public class ProductBuilder
{
    private int _id;
    private string _name = "Default Product Name";
    private string _description = "Default product description used for testing purposes.";
    private decimal _price = 9.99m;
    private int _stockQuantity = 10;
    private DateTime _created = DateTime.UtcNow;
    private DateTime _updated = DateTime.UtcNow;

    public ProductBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ProductBuilder WithStock(int quantity)
    {
        _stockQuantity = quantity;
        return this;
    }

    public ProductBuilder WithCreated(DateTime created)
    {
        _created = created;
        return this;
    }

    public ProductBuilder WithUpdated(DateTime updated)
    {
        _updated = updated;
        return this;
    }

    public Product Build()
    {
        return new Product
        {
            Id = _id,
            Name = _name,
            Description = _description,
            Price = _price,
            Stock = Stock.Create(_stockQuantity),
            Created = _created,
            Updated = _updated
        };
    }
}
