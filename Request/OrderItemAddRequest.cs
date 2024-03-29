﻿using System.ComponentModel.DataAnnotations;

namespace OrdersMinimalAPI.Request
{
    public class OrderItemAddRequest
    {
        [Required, MaxLength(50)]
        public string? ProductName { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Range(1, int.MaxValue)]
        public int UnitPrice { get; set; }
    }
}
