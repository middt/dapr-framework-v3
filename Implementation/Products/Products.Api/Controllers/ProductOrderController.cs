using Dapr.Framework.Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Services;
using Products.Domain.Entities;

namespace Products.Api.Controllers;

[ApiVersion("1.0")]
public class ProductOrderController : BaseController<Product, ProductService>
{
    private readonly IProductOrderService _productOrderService;

    public ProductOrderController(
        ProductService service, 
        IProductOrderService productOrderService) : base(service, "product-multiple-transactions")
    {
        _productOrderService = productOrderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProductWithOrder()
    {
        var result = await _productOrderService.CreateProductWithOrderAsync();
        return Ok(result);
    }

    [HttpPut("{productId}/orders/{orderId}")]
    public async Task<IActionResult> UpdateProductAndOrder(
        string productId, 
        string orderId, 
        [FromBody] decimal newPrice)
    {
        var result = await _productOrderService.UpdateProductAndOrderAsync(productId, orderId, newPrice);
        return Ok(result);
    }
}
