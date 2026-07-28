using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class ProductRepository(HandwerkerDbContext db) : IProductRepository
{
    public async Task<IEnumerable<Product>> GetAllAsync(int page, int pageSize)
        => await db.Products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    public async Task<Product?> GetByIdAsync(int id)
        => await db.Products.FindAsync(id);

    public async Task<bool> ExistsAsync(int id)
        => await db.Products.AnyAsync(p => p.Id == id);

    public async Task AddAsync(Product product)
        => await db.Products.AddAsync(product);

    public void Update(Product product)
    {
        // Stelle sicher, dass das Entity nicht bereits getrackt wird
        var tracked = db.ChangeTracker.Entries<Product>()
            .FirstOrDefault(e => e.Entity.Id == product.Id);
        
        if (tracked != null)
        {
            db.Entry(tracked.Entity).State = EntityState.Detached;
        }
        
        db.Products.Update(product);
    }

    public void Remove(Product product)
        => db.Products.Remove(product);

    public async Task SaveChangesAsync()
        => await db.SaveChangesAsync();
        
    public async Task<IEnumerable<Product>> SearchAsync(string expression)
        => await db.Products
            .Where(p => p.Name.ToLower().Contains(expression.ToLower()) || p.ArticleNumber.ToLower().Contains(expression.ToLower()))            
            .Take(20)
            .ToListAsync();

    public async Task<int> CountAsync(CancellationToken cancellationToken)
        => await db.Products.CountAsync(cancellationToken);
}