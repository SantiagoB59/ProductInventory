
namespace Products.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    private Product()
    {
        
    }

    public Product(string name, string? description, decimal price, int stock)
    {
        Id = Guid.NewGuid();

        ValidateName(name);
        ValidatePrice(price);
        ValidateStock(stock);

        Name = name.Trim();
        Description = NormalizeDescription(description);
        Price = price;
        Stock = stock;
    }

    public void Update(string name, string? description, decimal price)
    {
        ValidateName(name);
        ValidatePrice(price);

        Name = name.Trim();
        Description = NormalizeDescription(description);
        Price = price;
    }

    public void AdjustStock(int quantity)
    {
        if (quantity == 0)
        {
            throw new ArgumentException(
                "La cantidad a ajustar debe ser diferente de cero.",
                nameof(quantity));
        }

        long newStock = (long)Stock + quantity;

        if (newStock < 0)
        {
            throw new InvalidOperationException(
                "No hay existencias suficientes para realizar el ajuste.");
        }

        if (newStock > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "El inventario supera el límite permitido.");
        }

        Stock = (int)newStock;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre del producto es obligatorio.",
                nameof(name));
        }

        if (name.Trim().Length > 120)
        {
            throw new ArgumentException(
                "El nombre no puede superar los 120 caracteres.",
                nameof(name));
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "El precio debe ser mayor que cero.");
        }
    }

    private static void ValidateStock(int stock)
    {
        if (stock < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stock),
                "El inventario inicial no puede ser negativo.");
        }
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        string normalized = description.Trim();

        if (normalized.Length > 500)
        {
            throw new ArgumentException(
                "La descripción no puede superar los 500 caracteres.",
                nameof(description));
        }

        return normalized;
    }
}
