using Microsoft.AspNetCore.Mvc;
using ZeissAssessment.Application.Contracts.Products;
using ZeissAssessment.Application.Interfaces.Services;

namespace ZeissAssessment.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet("{productId:int}")]
    public async Task<ActionResult<ProductResponse>> GetById([FromRoute] int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await productService.GetByIdAsync(productId, cancellationToken);

        return Ok(product);
    }

    [HttpGet()]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(CancellationToken cancellationToken = default)
    {
        var products = await productService.GetAllAsync(cancellationToken);

        return Ok(products);
    }

    [HttpPost()]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await productService.CreateAsync(request, cancellationToken);

        return Ok(product);
    }

    [HttpPut("{productId:int}")]
    public async Task<ActionResult<ProductResponse>> Update([FromRoute] int productId,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await productService.UpdateAsync(productId, request, cancellationToken);
        return Ok(product);
    }

    [HttpDelete("{productId:int}")]
    public async Task<ActionResult> Delete([FromRoute] int productId, CancellationToken cancellationToken = default)
    {
        await productService.RemoveAsync(productId, cancellationToken);

        return NoContent();
    }

    [HttpPost("{productId:int}/add-to-stock/{quantity:int}")]
    public async Task<ActionResult<ProductResponse>> AddToStock([FromRoute] int productId, [FromRoute] int quantity,
        CancellationToken cancellationToken = default)
    {
        var response = await productService.IncrementStock(productId, quantity, cancellationToken);

        return Ok(response);
    }

    [HttpPost("{productId:int}/decrement-stock/{quantity:int}")]
    public async Task<ActionResult<ProductResponse>> DecrementStock([FromRoute] int productId, [FromRoute] int quantity,
        CancellationToken cancellationToken = default)
    {
        var response = await productService.DecrementStock(productId, quantity, cancellationToken);

        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> Search([FromQuery] SearchProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await productService.Search(request, cancellationToken);

        return Ok(response);
    }

    [HttpGet("stock-level")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> StockLevelSearch(
        [FromQuery] StockLevelProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await productService.StockLevelSearch(request, cancellationToken);

        return Ok(response);
    }
}