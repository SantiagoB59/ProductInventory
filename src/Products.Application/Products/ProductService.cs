
using Products.Application.Contracts.Persistence;
using Products.Domain.Entities;

namespace Products.Application.Products;

public sealed class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Product> CreateAsync(
        string name,
        string? description,
        decimal price,
        int stock,
        CancellationToken cancellationToken = default)
    {
        var product = new Product(name, description, price, stock);

        await _repository.AddAsync(product, cancellationToken);

        return product;
    }

    public Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ProductPage> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page), "La página debe ser mayor que cero.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "El tamaño de página debe estar entre 1 y 100.");
        }

        var products = await _repository.GetAllAsync(
            page, pageSize, cancellationToken);

        var total = await _repository.CountAsync(cancellationToken);

        return new ProductPage(products, page, pageSize, total);
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        string name,
        string? description,
        decimal price,
        CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return false;
        }

        product.Update(name, description, price);

        return await _repository.UpdateAsync(product, cancellationToken);
    }

    public Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }

    public Task<StockAdjustmentResult> AdjustStockAsync(
        Guid id,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity == 0)
        {
            return Task.FromResult(StockAdjustmentResult.InvalidQuantity);
        }

        return _repository.AdjustStockAsync(id, quantity, cancellationToken);
    }
}

public sealed record ProductPage(
    IReadOnlyList<Product> Items,
    int Page,
    int PageSize,
    int TotalItems);
