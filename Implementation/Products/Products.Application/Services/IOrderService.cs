using System.Threading.Tasks;
using Products.Domain.Entities;

namespace Products.Application.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Order?> GetOrderWithDetailsAsync(string orderId);
    }
}
