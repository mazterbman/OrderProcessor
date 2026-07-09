using Microsoft.EntityFrameworkCore;
using OrderProcessor.Domain;

namespace OrderProcessor.Infrastructure;

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public OrderDbContext(DbContextOptions<OrderDbContext> options) 
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Order>().OwnsMany(o => o.Items, item =>
        {
            item.WithOwner();
        });

        modelBuilder.Entity<Order>().Navigation(o => o.Items).HasField("_orderItems")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}