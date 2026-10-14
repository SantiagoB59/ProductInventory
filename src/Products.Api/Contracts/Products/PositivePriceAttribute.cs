using System.ComponentModel.DataAnnotations;

namespace Products.Api.Contracts.Products;

/// <summary>
/// Validates that a price is within the permitted positive range.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class PositivePriceAttribute : ValidationAttribute
{
    private const decimal MinimumPrice = 0.01m;
    private const decimal MaximumPrice = 9_999_999_999_999_999.99m;

    /// <summary>
    /// Determines whether the specified value is a valid price.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>
    /// True if the value is a decimal within the permitted range; otherwise, false.
    /// </returns>
    public override bool IsValid(object? value)
    {
        return value is decimal price
            && price >= MinimumPrice
            && price <= MaximumPrice;
    }

    /// <summary>
    /// Formats the validation error message for the specified member.
    /// </summary>
    /// <param name="name">The name of the member being validated.</param>
    /// <returns>The formatted validation error message.</returns>
    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be between 0.01 and 9999999999999999.99.";
    }
}