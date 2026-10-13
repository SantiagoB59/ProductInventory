
using Microsoft.EntityFrameworkCore;
using Products.Application.Contracts.Persistence;
using Products.Domain.Entities;
using Products.Infrastructure.Data;

namespace Products.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return _context.Products.CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Products
            .AnyAsync(p => p.Id == product.Id, cancellationToken);

        if (!exists)
        {
            return false;
        }

        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<StockAdjustmentResult> AdjustStockAsync(
        Guid id,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity == 0)
        {
            return StockAdjustmentResult.InvalidQuantity;
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        var product = await _context.Products
            .FromSqlInterpolated(
                $"SELECT * FROM products WHERE \"Id\" = {id} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return StockAdjustmentResult.NotFound;
        }

        var newStock = (long)product.Stock + quantity;

        if (newStock < 0)
        {
            return StockAdjustmentResult.InsufficientStock;
        }

        if (newStock > int.MaxValue)
        {
            return StockAdjustmentResult.StockLimitExceeded;
        }

        product.AdjustStock(quantity);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return StockAdjustmentResult.Updated;
    }
}
