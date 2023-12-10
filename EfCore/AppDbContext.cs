using Microsoft.EntityFrameworkCore;
using OrdersMinimalAPI.Model;

namespace OrdersMinimalAPI.EfCore
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<Order> Orders => Set<Order>();
        public virtual DbSet<OrderItem> OrderItems => Set<OrderItem>();
    }
}
