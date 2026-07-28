using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface IRecipientRepository
{
    Task<IEnumerable<Recipient>> GetAllAsync();
    Task<Recipient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id);
    Task AddAsync(Recipient recipient);
    void Update(Recipient recipient);
    void Remove(Recipient recipient);
    Task SaveChangesAsync();
    Task<int> CountAsync(CancellationToken cancellationToken);
}