
using System.ComponentModel.DataAnnotations;

namespace Products.Api.Contracts.Products;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class PositivePriceAttribute : ValidationAttribute
{
    private const decimal MinimumPrice = 0.01m;
    private const decimal MaximumPrice = 9_999_999_999_999_999.99m;

    public override bool IsValid(object? value)
    {
        return value is decimal price
            && price >= MinimumPrice
            && price <= MaximumPrice;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be between 0.01 and 9999999999999999.99.";
    }
}
