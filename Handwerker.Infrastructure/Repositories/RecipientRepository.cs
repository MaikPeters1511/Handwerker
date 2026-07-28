﻿using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class RecipientRepository(HandwerkerDbContext db) : IRecipientRepository
{
    public async Task<IEnumerable<Recipient>> GetAllAsync()
        => await db.Recipients.ToListAsync();

    public async Task<Recipient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.Recipients.FindAsync([id], cancellationToken);

    public async Task<bool> ExistsAsync(int id)
        => await db.Recipients.AnyAsync(r => r.Id == id);

    public async Task AddAsync(Recipient recipient)
        => await db.Recipients.AddAsync(recipient);

    public void Update(Recipient recipient)
    {
        // Detache bereits getracktes Entity (falls vorhanden) - gleiche Logik wie ProductRepository
        var tracked = db.ChangeTracker.Entries<Recipient>()
            .FirstOrDefault(e => e.Entity.Id == recipient.Id);
        
        if (tracked != null)
        {
            db.Entry(tracked.Entity).State = EntityState.Detached;
        }
        
        db.Recipients.Update(recipient);
    }

    public void Remove(Recipient recipient)
        => db.Recipients.Remove(recipient);

    public async Task SaveChangesAsync()
        => await db.SaveChangesAsync();

    public async Task<int> CountAsync(CancellationToken cancellationToken)
        => await db.Recipients.CountAsync(cancellationToken);
}