using Dapr.Framework.Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Services;
using Products.Domain.Entities;
using System.Diagnostics;

namespace Products.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductControllerV2 : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ActivitySource _activitySource;
    private readonly ExternalApiService _externalApiService;

    public ProductControllerV2(
        ProductService productService, 
        ExternalApiService externalApiService)
    {
        _productService = productService ?? 
            throw new ArgumentNullException(nameof(productService));
        _activitySource = new ActivitySource("Products.Api.Controllers.V2", "2.0.0");
        _externalApiService = externalApiService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestV2 request)
    {
        if (request == null)
        {
            return BadRequest("Product details are required");
        }

        using var activity = _activitySource.StartActivity("CreateProduct");

        var product = new Product
        {
            Id = request.Id ?? Guid.NewGuid().ToString(), // Use provided ID or generate new
            Name = request.Name ?? string.Empty,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive
        };

        var createdProduct = await _productService.CreateAsync(product);

        return CreatedAtAction(
            nameof(GetProductById), 
            new { version = "2.0", id = createdProduct.Id }, 
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

    [HttpPost("external")]
    public async Task<IActionResult> TestExternalService([FromBody] string message)
    {
        using var activity = _activitySource.StartActivity("ExternalServiceCall");
        activity?.SetTag("message", message);

        try
        {
            var result = await _externalApiService.GetExternalDataAsync(message);
            activity?.SetTag("success", result != null);
            
            return result is not null
                ? Ok(new { 
                    Message = result, 
                    Version = "2.0", 
                    AdditionalInfo = "Enhanced external service call" 
                })
                : NotFound();
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Request model for creating a new product in V2
/// </summary>
public class CreateProductRequestV2
{
    /// <summary>
    /// Optional Product ID (will generate if not provided)
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string? Name { get; set; }

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
