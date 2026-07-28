// Handwerker.Infrastructure/Repositories/WageTypeRepository.cs
using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class WageTypeRepository(HandwerkerDbContext context) : IWageTypeRepository
{
    public async Task<WageType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.WageTypes
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WageType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.WageTypes
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WageType>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.WageTypes
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WageType>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await context.WageTypes
            .Where(w => w.IsActive && (
                w.Name.ToLower().Contains(term) ||
                w.WageNumber.ToLower().Contains(term) ||
                (w.Description != null && w.Description.ToLower().Contains(term))
            ))
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<WageType> AddAsync(WageType wageType, CancellationToken cancellationToken = default)
    {
        context.WageTypes.Add(wageType);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Entity bleibt sonst im "Added"-Status hängen und ein erneuter Add-Versuch
            // mit demselben Objekt (z.B. nach einer Kollision bei der Nummernvergabe) schlägt fehl.
            context.Entry(wageType).State = EntityState.Detached;
            throw;
        }

        return wageType;
    }

    public async Task UpdateAsync(WageType wageType, CancellationToken cancellationToken = default)
    {
        wageType.UpdatedAt = DateTime.UtcNow;
        context.WageTypes.Update(wageType);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var wageType = await context.WageTypes.FindAsync(new object[] { id }, cancellationToken);
        if (wageType != null)
        {
            wageType.IsActive = false;
            wageType.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string wageNumber, CancellationToken cancellationToken = default)
    {
        return await context.WageTypes
            .AnyAsync(w => w.WageNumber == wageNumber, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await context.WageTypes.CountAsync(cancellationToken);
    }
}
