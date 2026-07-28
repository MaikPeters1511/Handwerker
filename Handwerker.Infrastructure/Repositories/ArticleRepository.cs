using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class ArticleRepository(HandwerkerDbContext context) : IArticleRepository
{
    public async Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .Include(a => a.ArticleWarehouses)
            .ThenInclude(aw => aw.Warehouse)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Article?> GetByArticleNumberAsync(string articleNumber, CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .Include(a => a.ArticleWarehouses)
            .ThenInclude(aw => aw.Warehouse)
            .FirstOrDefaultAsync(a => a.ArticleNumber == articleNumber, cancellationToken);
    }

    public async Task<IEnumerable<Article>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .Include(a => a.ArticleWarehouses)
            .ThenInclude(aw => aw.Warehouse)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Article>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .Where(a => a.IsActive)
            .Include(a => a.ArticleWarehouses)
            .ThenInclude(aw => aw.Warehouse)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Article>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var articles = await context.Articles
            .Include(a => a.ArticleWarehouses)
            .ThenInclude(aw => aw.Warehouse)
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        // Filtere Artikel mit niedrigem Bestand in mindestens einem Lager
        return articles.Where(a => a.ArticleWarehouses.Any(aw => aw.IsLowStock));
    }

    public async Task<IEnumerable<Article>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await context.Articles
            .Where(a => a.IsActive && (
                a.Name.ToLower().Contains(term) ||
                a.ArticleNumber.ToLower().Contains(term) ||
                (a.Description != null && a.Description.ToLower().Contains(term)) ||
                (a.Category != null && a.Category.ToLower().Contains(term))
            ))
            .Include(a => a.ArticleWarehouses)
            .ThenInclude(aw => aw.Warehouse)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Article> AddAsync(Article article, CancellationToken cancellationToken = default)
    {
        context.Articles.Add(article);
        await context.SaveChangesAsync(cancellationToken);
        return article;
    }

    public async Task UpdateAsync(Article article, CancellationToken cancellationToken = default)
    {
        article.UpdatedAt = DateTime.UtcNow;
        context.Articles.Update(article);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var article = await context.Articles.FindAsync(new object[] { id }, cancellationToken);
        if (article != null)
        {
            // Soft delete - deaktivieren statt löschen
            article.IsActive = false;
            article.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string articleNumber, CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .AnyAsync(a => a.ArticleNumber == articleNumber, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await context.Articles.CountAsync(cancellationToken);
    }
}
