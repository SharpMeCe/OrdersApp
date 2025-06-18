using Microsoft.EntityFrameworkCore;
using OrdersApp.Domain.Entities;

namespace OrdersApp.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductName).HasColumnName("product_name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.OrderDate).HasColumnName("order_date");
            entity.Property(e => e.SourceEmail).HasColumnName("source_email");
        });
    }

}
