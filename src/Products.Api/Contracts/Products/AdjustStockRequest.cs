using System.ComponentModel.DataAnnotations;

namespace Products.Api.Contracts.Products;

/// <summary>
/// Represents a request to increase or decrease a product's stock.
/// </summary>
public sealed class AdjustStockRequest
{
    /// <summary>
    /// Gets the quantity to apply to the current stock.
    /// Positive values increase stock; negative values decrease it.
    /// </summary>
    [Range(-2147483647, 2147483647)]
    public int Quantity { get; init; }
}