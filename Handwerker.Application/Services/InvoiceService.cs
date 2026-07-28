using Handwerker.Domain.Interfaces;
using Handwerker.Domain.Entities;

namespace Handwerker.Application.Services;

public class InvoiceService(IInvoiceRepository  repo)
{
    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return await repo.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(cancellationToken);
    }
}

