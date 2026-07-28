using Handwerker.Application.Abstractions;
using Handwerker.Domain.Entities;

namespace Handwerker.Application.Features.Invoices.Commands;

// ── Request-DTOs ──────────────────────────────────────────────────────────────

public sealed record ProductLineDto(
    string ArticleNumber,
    string Name,
    int Position,
    double Quantity,
    string Unit,
    string Description,
    decimal TaxRate,
    decimal TaxAmount,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal TotalNet,
    decimal TotalGross);

// ── Commands ──────────────────────────────────────────────────────────────────

/// <summary>Neue Rechnung anlegen.</summary>
public sealed record CreateInvoiceCommand(
    string UserId,
    int RecipientId,
    int ProviderId,
    string? InvoiceNumber,
    DateTime InvoiceDate,
    DateTime DueDate,
    string ServicePeriod,
    string CustomerNumber,
    string PaymentTerms,
    string IntroText,
    string OutroText,
    decimal TotalNet,
    decimal TotalTaxAmount,
    decimal TotalGross,
    IReadOnlyList<ProductLineDto> Products) : ICommand<int>;

/// <summary>Bestehende Rechnung aktualisieren.</summary>
public sealed record UpdateInvoiceCommand(
    int Id,
    string UserId,
    int RecipientId,
    int ProviderId,
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
    IReadOnlyList<ProductLineDto> Products) : ICommand;

/// <summary>Rechnung löschen.</summary>
public sealed record DeleteInvoiceCommand(int Id, string UserId) : ICommand;

/// <summary>Rechnung als bezahlt/unbezahlt markieren.</summary>
public sealed record MarkInvoicePaidCommand(int Id, string UserId, bool IsPaid) : ICommand;

/// <summary>Rechnung aus einem Angebot erzeugen.</summary>
public sealed record ConvertOfferToInvoiceCommand(
    int OfferId,
    string UserId,
    bool IncludeOfferLines = true) : ICommand<int>;

