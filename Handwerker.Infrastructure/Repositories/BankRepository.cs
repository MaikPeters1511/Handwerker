using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class BankRepository(HandwerkerDbContext db) : IBankRepository
{
    public async Task<IEnumerable<Bank>> GetAllAsync()
        => await db.Banks.Take(100).ToListAsync();

    public async Task<Bank?> GetByIdAsync(int id)
        => await db.Banks.FindAsync(id);

    public async Task<bool> ExistsAsync(int id)
        => await db.Banks.AnyAsync(b => b.Id == id);

    public async Task AddAsync(Bank bank)
        => await db.Banks.AddAsync(bank);

    public void Update(Bank bank)
    {
        var tracked = db.ChangeTracker.Entries<Bank>()
            .FirstOrDefault(e => e.Entity.Id == bank.Id);
        
        if (tracked != null)
        {
            db.Entry(tracked.Entity).State = EntityState.Detached;
        }
        
        db.Banks.Update(bank);
    }

    public void Remove(Bank bank)
        => db.Banks.Remove(bank);

    public async Task<bool> ExistsByIbanAsync(string iban)
        => await db.Banks.AnyAsync(b => b.Iban == iban);

    public async Task SaveChangesAsync()
        => await db.SaveChangesAsync();

    public async Task<IEnumerable<Bank>> SearchAsync(string expression)
        => await db.Banks
            .Where(b => b.Name.ToLower().Contains(expression) || b.Iban.Contains(expression) || b.Bic.ToLower().Contains(expression))
            .Take(20)
            .ToListAsync();
 
}