using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<Provider>> GetAllAsync(int page, int pageSize);
    Task AddAsync(Provider provider);
    void Update(Provider provider);
    void Remove(Provider provider);
    Task SaveChangesAsync();
    Task<IEnumerable<Provider>> SearchAsync(string expression);
    Task<int> CountAsync(CancellationToken cancellationToken);
}