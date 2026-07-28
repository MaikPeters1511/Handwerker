using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class ServiceItemRepository(HandwerkerDbContext context) : IServiceItemRepository
{
    public async Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.ServiceItems
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ServiceItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ServiceItems
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceItem>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.ServiceItems
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceItem>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await context.ServiceItems
            .Where(s => s.IsActive && (
                s.Name.ToLower().Contains(term) ||
                s.ServiceNumber.ToLower().Contains(term) ||
                (s.Description != null && s.Description.ToLower().Contains(term))
            ))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceItem> AddAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default)
    {
        context.ServiceItems.Add(serviceItem);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Entity bleibt sonst im "Added"-Status hängen und ein erneuter Add-Versuch
            // mit demselben Objekt (z.B. nach einer Kollision bei der Nummernvergabe) schlägt fehl.
            context.Entry(serviceItem).State = EntityState.Detached;
            throw;
        }

        return serviceItem;
    }

    public async Task UpdateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default)
    {
        serviceItem.UpdatedAt = DateTime.UtcNow;
        context.ServiceItems.Update(serviceItem);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var serviceItem = await context.ServiceItems.FindAsync(new object[] { id }, cancellationToken);
        if (serviceItem != null)
        {
            serviceItem.IsActive = false;
            serviceItem.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string serviceNumber, CancellationToken cancellationToken = default)
    {
        return await context.ServiceItems
            .AnyAsync(s => s.ServiceNumber == serviceNumber, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await context.ServiceItems.CountAsync(cancellationToken);
    }
}
