
using Products.Domain.Entities;

namespace Products.Tests;

public class ProductTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesProduct()
    {
        var product = new Product(
            "  Laptop empresarial  ",
            "  Equipo de trabajo  ",
            2500000m,
            10);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Laptop empresarial", product.Name);
        Assert.Equal("Equipo de trabajo", product.Description);
        Assert.Equal(2500000m, product.Price);
        Assert.Equal(10, product.Stock);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ThrowsArgumentException(
        string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Product(name, null, 100m, 0));
    }

    [Fact]
    public void Constructor_WithNameLongerThan120Characters_ThrowsArgumentException()
    {
        var name = new string('A', 121);

        Assert.Throws<ArgumentException>(() =>
            new Product(name, null, 100m, 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidPrice_ThrowsArgumentOutOfRangeException(
        decimal price)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Product("Producto", null, price, 0));
    }

    [Fact]
    public void Constructor_WithNegativeStock_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Product("Producto", null, 100m, -1));
    }

    [Fact]
    public void AdjustStock_WithPositiveQuantity_IncreasesStock()
    {
        var product = new Product("Producto", null, 100m, 10);

        product.AdjustStock(5);

        Assert.Equal(15, product.Stock);
    }

    [Fact]
    public void AdjustStock_WithNegativeQuantity_DecreasesStock()
    {
        var product = new Product("Producto", null, 100m, 10);

        product.AdjustStock(-3);

        Assert.Equal(7, product.Stock);
    }

    [Fact]
    public void AdjustStock_WithZeroQuantity_ThrowsArgumentException()
    {
        var product = new Product("Producto", null, 100m, 10);

        Assert.Throws<ArgumentException>(() =>
            product.AdjustStock(0));
    }

    [Fact]
    public void AdjustStock_WithInsufficientStock_ThrowsInvalidOperationException()
    {
        var product = new Product("Producto", null, 100m, 2);

        Assert.Throws<InvalidOperationException>(() =>
            product.AdjustStock(-3));

        Assert.Equal(2, product.Stock);
    }

    [Fact]
    public void AdjustStock_AboveMaximumStock_ThrowsArgumentOutOfRangeException()
    {
        var product = new Product("Producto", null, 100m, int.MaxValue);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            product.AdjustStock(1));

        Assert.Equal(int.MaxValue, product.Stock);
    }

    [Fact]
    public void Update_WithValidData_UpdatesProductDetails()
    {
        var product = new Product("Producto anterior", null, 100m, 5);

        product.Update("  Producto nuevo  ", "  Descripción  ", 200m);

        Assert.Equal("Producto nuevo", product.Name);
        Assert.Equal("Descripción", product.Description);
        Assert.Equal(200m, product.Price);
        Assert.Equal(5, product.Stock);
    }
}
