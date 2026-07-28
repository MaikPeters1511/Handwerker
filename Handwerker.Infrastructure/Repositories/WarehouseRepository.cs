using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class WarehouseRepository(HandwerkerDbContext context) : IWarehouseRepository
{
    public async Task<Warehouse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Warehouses
            .Include(w => w.ArticleWarehouses)
            .ThenInclude(aw => aw.Article)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Warehouses
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.Warehouses
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Warehouse> AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync(cancellationToken);
        return warehouse;
    }

    public async Task UpdateAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        warehouse.UpdatedAt = DateTime.UtcNow;
        context.Warehouses.Update(warehouse);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var warehouse = await context.Warehouses.FindAsync(new object[] { id }, cancellationToken);
        if (warehouse != null)
        {
            // Soft delete
            warehouse.IsActive = false;
            warehouse.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ArticleWarehouse?> GetStockAsync(int articleId, int warehouseId, CancellationToken cancellationToken = default)
    {
        return await context.ArticleWarehouses
            .Include(aw => aw.Article)
            .Include(aw => aw.Warehouse)
            .FirstOrDefaultAsync(aw => aw.ArticleId == articleId && aw.WarehouseId == warehouseId, cancellationToken);
    }

    public async Task UpdateStockAsync(ArticleWarehouse articleWarehouse, CancellationToken cancellationToken = default)
    {
        articleWarehouse.LastUpdated = DateTime.UtcNow;

        var existing = await context.ArticleWarehouses
            .FirstOrDefaultAsync(aw => aw.ArticleId == articleWarehouse.ArticleId && aw.WarehouseId == articleWarehouse.WarehouseId, cancellationToken);

        if (existing != null)
        {
            existing.StockQuantity = articleWarehouse.StockQuantity;
            existing.MinStockLevel = articleWarehouse.MinStockLevel;
            existing.MaxStockLevel = articleWarehouse.MaxStockLevel;
            existing.StorageLocation = articleWarehouse.StorageLocation;
            existing.LastUpdated = DateTime.UtcNow;
            context.ArticleWarehouses.Update(existing);
        }
        else
        {
            context.ArticleWarehouses.Add(articleWarehouse);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
