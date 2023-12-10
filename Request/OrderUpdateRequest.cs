﻿using System.ComponentModel.DataAnnotations;

namespace OrdersMinimalAPI.Request
{
    public class OrderUpdateRequest
    {
        [Required, MaxLength(50)]
        public string? CustomerName { get; set; }
        [Range(1, int.MaxValue)]
        public int TotalAmount { get; set; }
    }
}
