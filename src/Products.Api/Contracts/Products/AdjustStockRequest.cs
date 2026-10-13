using System.ComponentModel.DataAnnotations;

namespace Products.Api.Contracts.Products;

public sealed class AdjustStockRequest
{
    [Range(-2147483647, 2147483647)]
    public int Quantity { get; init; }
}
