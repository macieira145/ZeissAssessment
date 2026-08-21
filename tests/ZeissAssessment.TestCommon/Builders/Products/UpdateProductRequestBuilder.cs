using ZeissAssessment.Application.Contracts.Products;

namespace ZeissAssessment.TestCommon.Builders.Products;

public class UpdateProductRequestBuilder
{
    private string _name = "Updated Product";
    private string _description = "Updated product description.";
    private decimal _price = 19.99m;

    public UpdateProductRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UpdateProductRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public UpdateProductRequestBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public UpdateProductRequest Build()
    {
        return new UpdateProductRequest
        {
            Name = _name,
            Description = _description,
            Price = _price
        };
    }
}
