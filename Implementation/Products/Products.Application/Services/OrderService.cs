using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Domain.Services;
using Products.Domain.Entities;

namespace Products.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ITransactionService _transactionService;
        private readonly ICRUDRepository<Order> _orderRepository;
        private readonly ICRUDRepository<OrderItem> _orderItemRepository;
        private readonly ICRUDRepository<Product> _productRepository;

        public OrderService(
            ITransactionService transactionService,
            ICRUDRepository<Order> orderRepository,
            ICRUDRepository<OrderItem> orderItemRepository,
            ICRUDRepository<Product> productRepository)
        {
            _transactionService = transactionService;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await _transactionService.BeginTransactionAsync();
            try
            {
                // Validate and get products
                foreach (var item in order.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null)
                        throw new InvalidOperationException($"Product with ID {item.ProductId} not found");
                    
                    if (product.StockQuantity < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
                    
                    // Update product stock
                    product.StockQuantity -= item.Quantity;
                    await _productRepository.UpdateAsync(product);
                    
                    // Set item price from current product price
                    item.UnitPrice = product.Price;
                }

                // Calculate total amount
                order.TotalAmount = order.Items.Sum(item => item.UnitPrice * item.Quantity);
                order.OrderDate = DateTime.UtcNow;
                order.Status = "Pending";

                // Create order
                var createdOrder = await _orderRepository.CreateAsync(order);

                // Create order items
                foreach (var item in order.Items)
                {
                    item.OrderId = createdOrder.Id;
                    await _orderItemRepository.CreateAsync(item);
                }

                await _transactionService.CommitTransactionAsync();
                return createdOrder;
            }
            catch
            {
                await _transactionService.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Order?> GetOrderWithDetailsAsync(string orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return null;

            // Get order items
            var orderItems = (await _orderItemRepository.GetAllAsync())
                .Where(item => item.OrderId == orderId)
                .ToList();

            order.Items = orderItems;
            return order;
        }
    }
}
