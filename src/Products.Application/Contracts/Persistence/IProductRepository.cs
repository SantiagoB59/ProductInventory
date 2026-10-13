
using Products.Domain.Entities;

namespace Products.Application.Contracts.Persistence;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<StockAdjustmentResult> AdjustStockAsync(
        Guid id,
        int quantity,
        CancellationToken cancellationToken = default);
}

public enum StockAdjustmentResult
{
    Updated,
    NotFound,
    InsufficientStock,
    InvalidQuantity,
    StockLimitExceeded
}
