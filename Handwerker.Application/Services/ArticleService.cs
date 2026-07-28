using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

/// <summary>
/// Implementierung des Artikel-Application-Service.
/// Kapselt alle Artikel-Operationen hinter einem stabilen Interface,
/// damit Controller keine direkten Repository-Abhängigkeiten benötigen.
/// </summary>
public class ArticleService(IArticleRepository articleRepository) : IArticleService
{
    public Task<IEnumerable<Article>> GetAllAsync(CancellationToken cancellationToken = default)
        => articleRepository.GetAllAsync(cancellationToken);

    public Task<IEnumerable<Article>> GetActiveAsync(CancellationToken cancellationToken = default)
        => articleRepository.GetActiveAsync(cancellationToken);

    public Task<IEnumerable<Article>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        => articleRepository.SearchAsync(searchTerm, cancellationToken);

    public Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => articleRepository.GetByIdAsync(id, cancellationToken);

    public Task<bool> ExistsAsync(string articleNumber, CancellationToken cancellationToken = default)
        => articleRepository.ExistsAsync(articleNumber, cancellationToken);

    public async Task<Article> CreateAsync(Article article, string createdBy, CancellationToken cancellationToken = default)
    {
        article.IsActive = true;
        return await articleRepository.AddAsync(article, cancellationToken);
    }

    public Task UpdateAsync(Article article, CancellationToken cancellationToken = default)
        => articleRepository.UpdateAsync(article, cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => articleRepository.DeleteAsync(id, cancellationToken);
}

