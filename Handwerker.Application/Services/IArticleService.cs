using Handwerker.Domain.Entities;

namespace Handwerker.Application.Services;

/// <summary>
/// Application-Service für Artikel-Verwaltung.
/// Controller injizieren ausschließlich dieses Interface — kein direkter Repository-Zugriff.
/// </summary>
public interface IArticleService
{
    Task<IEnumerable<Article>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Article>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Article>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string articleNumber, CancellationToken cancellationToken = default);
    Task<Article> CreateAsync(Article article, string createdBy, CancellationToken cancellationToken = default);
    Task UpdateAsync(Article article, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

