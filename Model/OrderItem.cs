using System.ComponentModel.DataAnnotations;

namespace OrdersMinimalAPI.Model
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)]
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
        public int TotalPrice { get; set; }
        public Guid OrderId { get; set; }
    }
}
