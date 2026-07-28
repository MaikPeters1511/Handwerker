using Handwerker.Domain.Entities;

namespace Handwerker.Application.Services;

/// <summary>
/// Application-Service für Lohnarten-Verwaltung.
/// Controller injizieren ausschließlich dieses Interface — kein direkter Repository-Zugriff.
/// </summary>
public interface IWageTypeService
{
    Task<IEnumerable<WageType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WageType>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WageType>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<WageType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string wageNumber, CancellationToken cancellationToken = default);
    Task<WageType> CreateAsync(WageType wageType, CancellationToken cancellationToken = default);
    Task UpdateAsync(WageType wageType, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
