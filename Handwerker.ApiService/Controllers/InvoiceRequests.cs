using Handwerker.Application.Features.Invoices.Commands;

namespace Handwerker.ApiService.Controllers;

// ── Request-Typen (API-Boundary) ──────────────────────────────────────────────

/// <summary>
/// Minimal-Referenz-Objekt: Frontend sendet recipient/provider als eingebettetes Objekt.
/// Nur die Id wird serverseitig ausgewertet.
/// </summary>
public sealed class EmbeddedIdRef
{
    public int Id { get; set; }
}

public sealed class CreateInvoiceRequest
{
    /// <summary>Flat-ID — wird gesetzt wenn Frontend recipientId direkt sendet.</summary>
    public int RecipientId { get; set; }

    /// <summary>Objekt-Variante — Frontend sendet recipient: { id: 1, ... }.</summary>
    public EmbeddedIdRef? Recipient
    {
        get => null;
        set { if (value?.Id > 0) RecipientId = value.Id; }
    }

    /// <summary>Flat-ID — wird gesetzt wenn Frontend providerId direkt sendet.</summary>
    public int ProviderId { get; set; }

    /// <summary>Objekt-Variante — Frontend sendet provider: { id: 1, ... }.</summary>
    public EmbeddedIdRef? Provider
    {
        get => null;
        set { if (value?.Id > 0) ProviderId = value.Id; }
    }

    public string? InvoiceNumber  { get; set; }
    public DateTime InvoiceDate   { get; set; }
    public DateTime DueDate       { get; set; }
    public string ServicePeriod   { get; set; } = string.Empty;
    public string CustomerNumber  { get; set; } = string.Empty;
    public string PaymentTerms    { get; set; } = string.Empty;
    public string IntroText       { get; set; } = string.Empty;
    public string OutroText       { get; set; } = string.Empty;
    public decimal TotalNet       { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalGross     { get; set; }
    public IReadOnlyList<ProductLineDto> Products { get; set; } = [];
}

public sealed class UpdateInvoiceRequest
{
    public int Id { get; set; }

    public int RecipientId { get; set; }

    public EmbeddedIdRef? Recipient
    {
        get => null;
        set { if (value?.Id > 0) RecipientId = value.Id; }
    }

    public int ProviderId { get; set; }

    public EmbeddedIdRef? Provider
    {
        get => null;
        set { if (value?.Id > 0) ProviderId = value.Id; }
    }

    public string InvoiceNumber   { get; set; } = string.Empty;
    public DateTime InvoiceDate   { get; set; }
    public DateTime DueDate       { get; set; }
    public string ServicePeriod   { get; set; } = string.Empty;
    public string CustomerNumber  { get; set; } = string.Empty;
    public string PaymentTerms    { get; set; } = string.Empty;
    public string IntroText       { get; set; } = string.Empty;
    public string OutroText       { get; set; } = string.Empty;
    public bool IsPaid            { get; set; }
    public decimal TotalNet       { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalGross     { get; set; }
    public IReadOnlyList<ProductLineDto> Products { get; set; } = [];
}

public sealed record MarkPaidRequest(bool IsPaid);


