using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;

namespace Handwerker.Application.Services;

public class ProductService(IProductRepository repo, HybridCache cache)
{
    private const string CacheKeyPrefix = "product:";
    private const string CacheKeyAll = "products:all";
    private const string CacheKeySearch = "products:search:";

    public async Task<IEnumerable<Product>> GetAsync(int page, int pageSize)
    {
        var cacheKey = $"{CacheKeyAll}:{page}:{pageSize}";

        return await cache.GetOrCreateAsync(
            cacheKey,
            async _ => await repo.GetAllAsync(page, pageSize),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            }
        );
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}{id}";

        return await cache.GetOrCreateAsync(
            cacheKey,
            async _ => await repo.GetByIdAsync(id),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            }
        );
    }

    public async Task<IEnumerable<Product>> SearchAsync(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return new List<Product>();
             
        expression = expression.ToLower();
        var cacheKey = $"{CacheKeySearch}{expression}";

        return await cache.GetOrCreateAsync(
            cacheKey,
            async _ => await repo.SearchAsync(expression),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(3),
                LocalCacheExpiration = TimeSpan.FromMinutes(3)
            }
        );
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return await repo.CountAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(int id)
    {
        var product = await repo.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException();

        repo.Remove(product);
        await repo.SaveChangesAsync();

        // Cache invalidieren
        await cache.RemoveAsync($"{CacheKeyPrefix}{id}");
        await InvalidateListCaches();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await repo.AddAsync(product);
        await repo.SaveChangesAsync();

        // Cache invalidieren
        await InvalidateListCaches();

        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        // Alternative: Prüfe Existenz ohne das Entity zu tracken
        if (!await repo.ExistsAsync(product.Id))
            throw new KeyNotFoundException("Product not found");

        repo.Update(product);
        await repo.SaveChangesAsync();
        
        // Cache invalidieren
        await cache.RemoveAsync($"{CacheKeyPrefix}{product.Id}");
        await InvalidateListCaches();

        return product;
    }

    private async Task InvalidateListCaches()
    {
        // Bei Create/Update/Delete alle Listen-Caches invalidieren
        // Pattern-basiertes Löschen ist mit HybridCache schwierig,
        // daher nutzen wir einen einfachen Ansatz mit bekannten Keys
        // Alternative: Cache mit kurzer TTL oder Tags verwenden
        await cache.RemoveAsync(CacheKeyAll);
        // Für Search-Cache könnte man Tags nutzen oder kurze TTL akzeptieren
    }
}