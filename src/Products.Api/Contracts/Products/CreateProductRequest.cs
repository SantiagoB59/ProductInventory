using System.ComponentModel.DataAnnotations;

namespace Products.Api.Contracts.Products;

public sealed class CreateProductRequest
{
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    [PositivePrice]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int Stock { get; init; }
}
