using System;
using Dapr.Framework.Domain.Common;

namespace Products.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public string OrderId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Product Product { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
}
