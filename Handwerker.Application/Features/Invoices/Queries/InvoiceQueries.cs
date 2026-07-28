using Handwerker.Application.Abstractions;
using Handwerker.Domain.Entities;

namespace Handwerker.Application.Features.Invoices.Queries;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public sealed record InvoiceListItemDto(
    int Id,
    string InvoiceNumber,
    DateTime InvoiceDate,
    DateTime DueDate,
    string CustomerNumber,
    string RecipientName,
    decimal TotalGross,
    bool IsPaid);

public sealed record InvoiceDetailDto(
    int Id,
    string InvoiceNumber,
    DateTime InvoiceDate,
    DateTime DueDate,
    string ServicePeriod,
    string CustomerNumber,
    string PaymentTerms,
    string IntroText,
    string OutroText,
    bool IsPaid,
    decimal TotalNet,
    decimal TotalTaxAmount,
    decimal TotalGross,
    Recipient Recipient,
    Provider Provider,
    IReadOnlyList<Product> Products);

// ── Queries ───────────────────────────────────────────────────────────────────

/// <summary>Seitengenaue, filterbare Rechnungsliste.</summary>
public sealed record GetInvoicesQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null,
    string? Status = null) : IQuery<IReadOnlyList<InvoiceListItemDto>>;

/// <summary>Einzelne Rechnung mit allen Details.</summary>
public sealed record GetInvoiceByIdQuery(int Id) : IQuery<InvoiceDetailDto?>;

/// <summary>Nächste freie Rechnungsnummer für einen Nutzer.</summary>
public sealed record GetNextInvoiceNumberQuery(string UserId) : IQuery<string>;

