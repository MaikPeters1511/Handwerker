using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetPagedAsync(int page, int pageSize, string? search, string? status, CancellationToken cancellationToken = default);
    Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}