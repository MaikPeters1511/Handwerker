using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class InvoiceRepository(HandwerkerDbContext db) : IInvoiceRepository
{
    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Invoices
            .Include(i => i.Products)
            .Include(i => i.Recipient)
            .Include(i => i.Provider)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await db.Invoices
            .Include(i => i.Products)
            .Include(i => i.Recipient)
            .Include(i => i.Provider)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Invoices
            .Include(i => i.Products)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetPagedAsync(
        int page, int pageSize, string? search, string? status, CancellationToken cancellationToken = default)
    {
        var query = db.Invoices
            .Include(i => i.Recipient)
            .Include(i => i.Provider)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(s) ||
                i.CustomerNumber.ToLower().Contains(s) ||
                i.Recipient.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = status.Trim().ToLower() switch
            {
                "paid"   => query.Where(i => i.IsPaid),
                "unpaid" => query.Where(i => !i.IsPaid),
                _        => query
            };
        }

        return await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        db.Invoices.Update(invoice);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await db.Invoices.FindAsync(new object[] { id }, cancellationToken);
        if (invoice != null)
        {
            db.Invoices.Remove(invoice);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await db.Invoices.CountAsync(cancellationToken);
}
