using System.Threading.Tasks;
using Products.Domain.Entities;

namespace Products.Application.Services
{
    public interface IProductOrderService
    {
        Task<(Product, Order)> CreateProductWithOrderAsync();
        Task<(Product, Order)> UpdateProductAndOrderAsync(string productId, string orderId, decimal newPrice);
    }
}
