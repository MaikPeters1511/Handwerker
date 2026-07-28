using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class OfferRepository(HandwerkerDbContext db) : IOfferRepository
{
    public async Task<IEnumerable<Offer>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.Offers
            .Include(o => o.Recipient)
            .Include(o => o.Provider)
            .Include(o => o.Products)
            .ToListAsync(cancellationToken);

    public async Task<Offer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.Offers
            .Include(o => o.Recipient)
            .Include(o => o.Provider)
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<Offer> AddAsync(Offer offer, CancellationToken cancellationToken = default)
    {
        await db.Offers.AddAsync(offer, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return offer;
    }

    public async Task UpdateAsync(Offer offer, CancellationToken cancellationToken = default)
    {
        // Detache bereits getracktes Entity (falls vorhanden)
        var tracked = db.ChangeTracker.Entries<Offer>()
            .FirstOrDefault(e => e.Entity.Id == offer.Id);
        
        if (tracked != null)
        {
            db.Entry(tracked.Entity).State = EntityState.Detached;
        }
        
        db.Offers.Update(offer);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var offer = await db.Offers.FindAsync([id], cancellationToken);
        if (offer != null)
        {
            db.Offers.Remove(offer);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await db.Offers.CountAsync(cancellationToken);

    public async Task<IEnumerable<Offer>> GetSentOffersAsync(CancellationToken cancellationToken = default)
        => await db.Offers
            .Include(o => o.Recipient)
            .Include(o => o.Provider)
            .Include(o => o.Products)
            .Where(o => !o.IsReceived)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Offer>> GetReceivedOffersAsync(CancellationToken cancellationToken = default)
        => await db.Offers
            .Include(o => o.Recipient)
            .Include(o => o.Provider)
            .Include(o => o.Products)
            .Where(o => o.IsReceived)
            .ToListAsync(cancellationToken);
}
