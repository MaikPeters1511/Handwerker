using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Warehouse> AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
    Task UpdateAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<ArticleWarehouse?> GetStockAsync(int articleId, int warehouseId, CancellationToken cancellationToken = default);
    Task UpdateStockAsync(ArticleWarehouse articleWarehouse, CancellationToken cancellationToken = default);
}
