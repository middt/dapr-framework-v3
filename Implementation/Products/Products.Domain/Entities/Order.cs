using System;
using System.Collections.Generic;
using Dapr.Framework.Domain.Common;

namespace Products.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string CustomerEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
