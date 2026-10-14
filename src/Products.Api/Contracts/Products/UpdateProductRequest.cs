using System.ComponentModel.DataAnnotations;

namespace Products.Api.Contracts.Products;

/// <summary>
/// Represents the data required to update a product's general information.
/// </summary>
public sealed class UpdateProductRequest
{
    /// <summary>
    /// Gets the product name.
    /// </summary>
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional product description.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; init; }

    /// <summary>
    /// Gets the product price.
    /// </summary>
    [PositivePrice]
    public decimal Price { get; init; }
}