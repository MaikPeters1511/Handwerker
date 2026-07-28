// Handwerker.Domain/Interfaces/IWageTypeRepository.cs
using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface IWageTypeRepository
{
    Task<WageType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WageType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WageType>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WageType>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<WageType> AddAsync(WageType wageType, CancellationToken cancellationToken = default);
    Task UpdateAsync(WageType wageType, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string wageNumber, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
