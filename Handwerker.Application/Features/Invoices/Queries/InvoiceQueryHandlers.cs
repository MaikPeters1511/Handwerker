using Handwerker.Application.Abstractions;
using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Features.Invoices.Queries;

internal sealed class GetInvoicesHandler(IInvoiceRepository invoiceRepository)
    : IQueryHandler<GetInvoicesQuery, IReadOnlyList<InvoiceListItemDto>>
{
    public async Task<IReadOnlyList<InvoiceListItemDto>> HandleAsync(
        GetInvoicesQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var invoices = await invoiceRepository.GetPagedAsync(
            page, pageSize, query.Search, query.Status, cancellationToken);

        return invoices
            .Select(i => new InvoiceListItemDto(
                i.Id,
                i.InvoiceNumber,
                i.InvoiceDate,
                i.DueDate,
                i.CustomerNumber,
                i.Recipient.Name,
                i.TotalGross,
                i.IsPaid))
            .ToList()
            .AsReadOnly();
    }
}

internal sealed class GetInvoiceByIdHandler(IInvoiceRepository invoiceRepository)
    : IQueryHandler<GetInvoiceByIdQuery, InvoiceDetailDto?>
{
    public async Task<InvoiceDetailDto?> HandleAsync(
        GetInvoiceByIdQuery query, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(query.Id, cancellationToken);
        if (invoice is null) return null;

        return new InvoiceDetailDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.DueDate,
            invoice.ServicePeriod,
            invoice.CustomerNumber,
            invoice.PaymentTerms,
            invoice.IntroText,
            invoice.OutroText,
            invoice.IsPaid,
            invoice.TotalNet,
            invoice.TotalTaxAmount,
            invoice.TotalGross,
            invoice.Recipient,
            invoice.Provider,
            invoice.Products.AsReadOnly());
    }
}

internal sealed class GetNextInvoiceNumberHandler(ISettingsRepository settingsRepository)
    : IQueryHandler<GetNextInvoiceNumberQuery, string>
{
    public async Task<string> HandleAsync(
        GetNextInvoiceNumberQuery query, CancellationToken cancellationToken = default)
    {
        var settings = await settingsRepository.GetSettingsAsync(query.UserId, cancellationToken);
        var year = DateTime.UtcNow.Year;

        return settings is not null
            ? $"{settings.InvoicePrefix}{year}-{settings.NextInvoiceNumber:D4}"
            : $"RE-{year}-{1001:D4}";
    }
}

