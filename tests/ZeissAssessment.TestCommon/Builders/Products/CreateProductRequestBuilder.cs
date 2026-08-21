using ZeissAssessment.Application.Contracts.Products;

namespace ZeissAssessment.TestCommon.Builders.Products;

public class CreateProductRequestBuilder
{
    private string _name = "Default Product";
    private string _description = "Default product description.";
    private decimal _price = 9.99m;
    private int _stock = 10;

    public CreateProductRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CreateProductRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CreateProductRequestBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public CreateProductRequestBuilder WithStock(int stock)
    {
        _stock = stock;
        return this;
    }

    public CreateProductRequest Build()
    {
        return new CreateProductRequest
        {
            Name = _name,
            Description = _description,
            Price = _price,
            Stock = _stock
        };
    }
}
