using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface IServiceItemRepository
{
    Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceItem>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceItem>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<ServiceItem> AddAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string serviceNumber, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
