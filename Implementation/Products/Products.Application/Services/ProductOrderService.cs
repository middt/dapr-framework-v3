using System;
using System.Threading.Tasks;
using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Domain.Services;
using Products.Domain.Entities;

namespace Products.Application.Services
{
    public class ProductOrderService : IProductOrderService
    {
        private readonly ITransactionService _transactionService;
        private readonly ICRUDRepository<Product> _productRepo;
        private readonly ICRUDRepository<Order> _orderRepo;

        public ProductOrderService(
            ITransactionService transactionService,
            ICRUDRepository<Product> productRepo,
            ICRUDRepository<Order> orderRepo)
        {
            _transactionService = transactionService;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
        }

        public async Task<(Product, Order)> CreateProductWithOrderAsync()
        {
            await   _transactionService.BeginTransactionAsync();
            try
            {
                // Create a new product
                var product = await _productRepo.CreateAsync(new Product 
                { 
                    Name = "Sample Product",
                    Description = "A product created in transaction",
                    Price = 29.99m,
                    StockQuantity = 100,
                    IsActive = true
                }, saveChanges: false);

                // Create a related order
                var order = await _orderRepo.CreateAsync(new Order 
                {
                    CustomerEmail = "customer@example.com",
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = product.Price,
                    Status = "Created",
                    Items = new System.Collections.Generic.List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = product.Id,
                            Quantity = 1,
                            UnitPrice = product.Price
                        }
                    }
                });

                await _transactionService.CommitTransactionAsync();
                return (product, order);
            }
            catch
            {
                await _transactionService.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<(Product, Order)> UpdateProductAndOrderAsync(string productId, string orderId, decimal newPrice)
        {
            await _transactionService.BeginTransactionAsync();
            try
            {
                // Update product price
                var product = await _productRepo.GetByIdAsync(productId);
                if (product == null)
                    throw new InvalidOperationException("Product not found");
                
                product.Price = newPrice;
                await _productRepo.UpdateAsync(product);

                // Update related order
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                    throw new InvalidOperationException("Order not found");

                order.TotalAmount = newPrice * order.Items.Count;
                await _orderRepo.UpdateAsync(order);

                await _transactionService.CommitTransactionAsync();
                return (product, order);
            }
            catch
            {
                await _transactionService.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
