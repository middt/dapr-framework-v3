using Dapr.Framework.Application.Services;
using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Domain.Services;
using Products.Domain.Entities;

namespace Products.Application.Services;

public class ProductService : CRUDDataService<Product, ICRUDRepository<Product>>
{
    private readonly IDistributedLockService _lockService;

    public ProductService(ICRUDRepository<Product> repository, IDistributedLockService lockService) : base(repository)
    {
        _lockService = lockService;
    }

    public virtual async Task<IEnumerable<Product>> GetActiveProducts()
    {
        var allProducts = await GetAllAsync();
        return allProducts.Where(p => p.IsActive);
    }

    public virtual async Task<Product?> UpdateWithLockAsync(Guid id, Product request)
    {
        var result = await _lockService.ExecuteWithLockAsync<Product?>(
            resourceId: $"product-{id}",
            function: async () =>
            {
                // Get existing product
                var existingProduct = await GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return null;
                }

                // Update product properties
                existingProduct.Name = request.Name;
                existingProduct.Description = request.Description;
                existingProduct.Price = request.Price;
                existingProduct.StockQuantity = request.StockQuantity;
                existingProduct.IsActive = request.IsActive;

                // Save the updated product
                return await UpdateAsync(id.ToString(), existingProduct);
            },
            expiryInSeconds: 30);

        return result;
    }
}
