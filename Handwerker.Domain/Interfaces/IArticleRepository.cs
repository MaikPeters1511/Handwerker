using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Article?> GetByArticleNumberAsync(string articleNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Article>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Article>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Article>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Article>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Article> AddAsync(Article article, CancellationToken cancellationToken = default);
    Task UpdateAsync(Article article, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string articleNumber, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
