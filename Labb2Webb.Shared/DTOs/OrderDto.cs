﻿using Labb2Webb.Shared.Enums;

namespace Labb2Webb.Shared.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Unhandled;
        public List<OrderItemDto> OrderItems { get; set; }
        public string CustomerEmail { get; set; }
    }
}
