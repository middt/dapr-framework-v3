using Dapr.Framework.Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Services;
using Products.Domain.Entities;
using System.Diagnostics;

namespace Products.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ActivitySource _activitySource;

    public ProductController(ProductService productService)
    {
        _productService = productService ?? 
            throw new ArgumentNullException(nameof(productService));
        _activitySource = new ActivitySource(nameof(ProductController));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (request == null)
        {
            return BadRequest("Product details are required");
        }

        using var activity = _activitySource.StartActivity("CreateProduct");

        var product = new Product
        {
            Id = Guid.NewGuid().ToString(), // Generate a new ID
            Name = request.Name ?? string.Empty,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive
        };

        var createdProduct = await _productService.CreateAsync(product);

        return CreatedAtAction(
            nameof(GetProductById), 
            new { version = "1.0", id = createdProduct.Id }, 
            createdProduct
        );
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(string id)
    {
        using var activity = _activitySource.StartActivity("GetProductById");

        var product = await _productService.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound($"Product with ID {id} not found");
        }

        return Ok(product);
    }
}

/// <summary>
/// Request model for creating a new product
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Current stock quantity
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Whether the product is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
