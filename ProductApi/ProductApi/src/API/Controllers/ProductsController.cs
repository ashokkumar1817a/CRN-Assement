using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Application.DTOs;
using ProductApi.Application.Interfaces;

namespace ProductApi.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[Authorize]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Get a paginated list of products.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] PaginationQuery query, CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductsAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a specific product by id.</summary>
    [HttpGet("{id:int}", Name = "GetProductById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        return Ok(product);
    }

    /// <summary>Create a new product.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var createdBy = User.Identity?.Name ?? "system";
        var product = await _productService.CreateProductAsync(dto, createdBy, cancellationToken);
        return CreatedAtRoute("GetProductById", new { id = product.Id, version = "1.0" }, product);
    }

    /// <summary>Update an existing product.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        int id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var modifiedBy = User.Identity?.Name ?? "system";
        var product = await _productService.UpdateProductAsync(id, dto, modifiedBy, cancellationToken);
        return Ok(product);
    }

    /// <summary>Delete a product.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        await _productService.DeleteProductAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Get items belonging to a product (related resource).</summary>
    [HttpGet("{id:int}/items", Name = "GetProductItems")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ItemDto>>> GetProductItems(int id, CancellationToken cancellationToken)
    {
        var items = await _productService.GetItemsForProductAsync(id, cancellationToken);
        return Ok(items);
    }

    /// <summary>Add an item to a product.</summary>
    [HttpPost("{id:int}/items")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> AddItemToProduct(
        int id, [FromBody] CreateItemDto dto, CancellationToken cancellationToken)
    {
        var item = await _productService.AddItemToProductAsync(id, dto, cancellationToken);
        return CreatedAtRoute("GetProductItems", new { id, version = "1.0" }, item);
    }
}
