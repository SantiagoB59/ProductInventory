
using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;

namespace Products.Infrastructure.Data;

public sealed class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var product = modelBuilder.Entity<Product>();

        product.ToTable("products");
        product.HasKey(p => p.Id);

        product.Property(p => p.Id)
            .ValueGeneratedNever();

        product.Property(p => p.Name)
            .HasMaxLength(120)
            .IsRequired();

        product.Property(p => p.Description)
            .HasMaxLength(500);

        product.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        product.Property(p => p.Stock)
            .IsRequired();
    }
}
