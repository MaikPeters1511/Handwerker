using Handwerker.Domain.Entities;

namespace Handwerker.Application.Services;

/// <summary>
/// Application-Service für Leistungs-Verwaltung.
/// Controller injizieren ausschließlich dieses Interface — kein direkter Repository-Zugriff.
/// </summary>
public interface IServiceItemService
{
    Task<IEnumerable<ServiceItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceItem>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceItem>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string serviceNumber, CancellationToken cancellationToken = default);
    Task<ServiceItem> CreateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
