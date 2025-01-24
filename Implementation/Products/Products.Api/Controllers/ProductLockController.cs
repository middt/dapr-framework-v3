using Dapr.Framework.Api.Controllers.Base;
using Dapr.Framework.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Services;
using Products.Domain.Entities;

namespace Products.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductLockController : BaseController<Product, ProductService>
{
    private readonly IDistributedLockService _lockService;
    private readonly ProductService _productService;

    public ProductLockController(
        ProductService service, 
        IDistributedLockService lockService) : base(service, "products-lock")
    {
        _lockService = lockService;
        _productService = service;
    }

    [HttpPost("lock")]
    public async Task<IActionResult> LockProduct([FromBody] LockRequest request)
    {
        if (string.IsNullOrEmpty(request.ResourceId))
        {
            return BadRequest("ResourceId is required");
        }

        var result = await _lockService.TryAcquireLockAsync(
            request.ResourceId,
            request.ExpiryInSeconds ?? 60);

        return Ok(new { Success = result });
    }

    [HttpPost("unlock")]
    public async Task<IActionResult> UnlockProduct([FromBody] UnlockRequest request)
    {
        if (string.IsNullOrEmpty(request.ResourceId))
        {
            return BadRequest("ResourceId is required");
        }

        var result = await _lockService.ReleaseLockAsync(request.ResourceId);
        return Ok(new { Success = result });
    }

    [HttpPut("{id}/demo-update")]
    public async Task<IActionResult> DemoUpdateProduct(
        string id, 
        [FromBody] UpdateProductRequest request)
    {
        if (!int.TryParse(id, out int productId))
        {
            return BadRequest("Invalid product ID format");
        }

        var product = new Product
        {
            Name = request.Name ?? string.Empty,
            Description = request.Description ?? string.Empty,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = true // Set default value for IsActive
        };

        var result = await _productService.UpdateWithLockAsync(productId, product);

        if (result == null)
        {
            return NotFound($"Product with ID {id} not found");
        }

        return Ok(result);
    }

    [HttpPost("{id}/demo-process")]
    public async Task<IActionResult> DemoProcessProduct(string id)
    {
var success = await _lockService.ExecuteWithLockAsync(
                resourceId: $"product-process-{id}",
                action: async () =>
                {
                    // Simulate a long-running process
                    await Task.Delay(5000);

                    // Simulate some work being done
                    await Task.Delay(1000); // Database operation
                    await Task.Delay(1000); // External API call
                    await Task.Delay(1000); // File processing
                },
                expiryInSeconds: 10);

            if (!success)
            {
                return Conflict();
            }

            return Ok(new { Message = "Process completed successfully" });
    }
}

// Request models (you might want to move these to a separate file)
public class LockRequest
{
    public string? ResourceId { get; set; }
    public int? ExpiryInSeconds { get; set; }
}

public class UnlockRequest
{
    public string? ResourceId { get; set; }
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
