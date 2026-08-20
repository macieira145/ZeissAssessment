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
}