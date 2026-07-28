using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class ProviderRepository(HandwerkerDbContext db) : IProviderRepository
{
    public async Task<IEnumerable<Provider>> GetAllAsync(int page, int pageSize)
        => await db.Providers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    
    public async Task<Provider?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.Providers.FindAsync([id], cancellationToken);

    public async Task<bool> ExistsAsync(int id)
        => await db.Providers.AnyAsync(p => p.Id == id);

    public async Task AddAsync(Provider product)
        => await db.Providers.AddAsync(product);

    public void Update(Provider provider)
    {
        var tracked = db.ChangeTracker.Entries<Provider>()
            .FirstOrDefault(e => e.Entity.Id == provider.Id);
        
        if (tracked != null)
        {
            db.Entry(tracked.Entity).State = EntityState.Detached;
        }
        
        db.Providers.Update(provider);
    }

    public void Remove(Provider product)
        => db.Providers.Remove(product);

    public async Task SaveChangesAsync()
        => await db.SaveChangesAsync();
        
    public async Task<IEnumerable<Provider>> SearchAsync(string expression)
        => await db.Providers
            .Where(p => p.Name.ToLower().Contains(expression.ToLower()) || p.Company.ToLower().Contains(expression.ToLower()))            
            .Take(20)
            .ToListAsync();

    public async Task<int> CountAsync(CancellationToken cancellationToken)
        => await db.Providers.CountAsync(cancellationToken);
}