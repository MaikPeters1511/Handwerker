using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Handwerker.Application.Abstractions;
using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Application.Services;

namespace Handwerker.Application.Features.Invoices.Commands;

internal sealed class CreateInvoiceHandler(
    IInvoiceRepository invoiceRepository,
    IRecipientRepository recipientRepository,
    IProviderRepository providerRepository,
    ISettingsRepository settingsRepository,
    NotificationService notificationService)
    : ICommandHandler<CreateInvoiceCommand, int>
{
    public async Task<int> HandleAsync(CreateInvoiceCommand cmd, CancellationToken cancellationToken = default)
    {
        var recipient = await recipientRepository.GetByIdAsync(cmd.RecipientId, cancellationToken)
            ?? throw new InvalidOperationException($"Empfänger mit ID {cmd.RecipientId} wurde nicht gefunden.");

        var provider = await providerRepository.GetByIdAsync(cmd.ProviderId, cancellationToken)
            ?? throw new InvalidOperationException($"Anbieter mit ID {cmd.ProviderId} wurde nicht gefunden.");

        var invoiceNumber = string.IsNullOrWhiteSpace(cmd.InvoiceNumber)
            ? await GenerateNextNumberAsync(cmd.UserId, cancellationToken)
            : cmd.InvoiceNumber;

        var invoice = new Invoice
        {
            InvoiceNumber  = invoiceNumber,
            InvoiceDate    = NormalizeUtc(cmd.InvoiceDate),
            DueDate        = NormalizeUtc(cmd.DueDate),
            ServicePeriod  = cmd.ServicePeriod,
            CustomerNumber = cmd.CustomerNumber,
            PaymentTerms   = cmd.PaymentTerms,
            IntroText      = cmd.IntroText,
            OutroText      = cmd.OutroText,
            TotalNet       = cmd.TotalNet,
            TotalTaxAmount = cmd.TotalTaxAmount,
            TotalGross     = cmd.TotalGross,
            IsPaid         = false,
            Recipient      = recipient,
            Provider       = provider,
            Products       = cmd.Products.Select(MapProduct).ToList()
        };

        var created = await invoiceRepository.AddAsync(invoice, cancellationToken);
        await notificationService.NotifyInvoiceCreatedAsync(cmd.UserId, created.Id, created.InvoiceNumber);

        return created.Id;
    }

    private async Task<string> GenerateNextNumberAsync(string userId, CancellationToken ct)
    {
        var settings = await settingsRepository.GetSettingsAsync(userId, ct);
        var year      = DateTime.UtcNow.Year;
        var number    = $"{settings.InvoicePrefix}{year}-{settings.NextInvoiceNumber:D4}";
        settings.NextInvoiceNumber++;
        await settingsRepository.UpdateSettingsAsync(settings, ct);
        return number;
    }

    private static Product MapProduct(ProductLineDto p) => new()
    {
        ArticleNumber   = p.ArticleNumber,
        Name            = p.Name,
        Position        = p.Position,
        Quantity        = p.Quantity,
        Unit            = p.Unit,
        Description     = p.Description,
        TaxRate         = p.TaxRate,
        TaxAmount       = p.TaxAmount,
        UnitPrice       = p.UnitPrice,
        DiscountPercent = p.DiscountPercent,
        DiscountAmount  = p.DiscountAmount,
        TotalNet        = p.TotalNet,
        TotalGross      = p.TotalGross
    };

    private static DateTime NormalizeUtc(DateTime dt) => dt.Kind switch
    {
        DateTimeKind.Utc   => dt,
        DateTimeKind.Local => dt.ToUniversalTime(),
        _                  => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
    };
}

// ─────────────────────────────────────────────────────────────────────────────

internal sealed class UpdateInvoiceHandler(
    IInvoiceRepository invoiceRepository,
    IRecipientRepository recipientRepository,
    IProviderRepository providerRepository,
    NotificationService notificationService)
    : ICommandHandler<UpdateInvoiceCommand>
{
    public async Task HandleAsync(UpdateInvoiceCommand cmd, CancellationToken cancellationToken = default)
    {
        var existing = await invoiceRepository.GetByIdAsync(cmd.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Rechnung {cmd.Id} nicht gefunden.");

        var recipient = await recipientRepository.GetByIdAsync(cmd.RecipientId, cancellationToken)
            ?? throw new InvalidOperationException($"Empfänger {cmd.RecipientId} nicht gefunden.");

        var provider = await providerRepository.GetByIdAsync(cmd.ProviderId, cancellationToken)
            ?? throw new InvalidOperationException($"Anbieter {cmd.ProviderId} nicht gefunden.");

        existing.InvoiceNumber  = cmd.InvoiceNumber;
        existing.InvoiceDate    = NormalizeUtc(cmd.InvoiceDate);
        existing.DueDate        = NormalizeUtc(cmd.DueDate);
        existing.ServicePeriod  = cmd.ServicePeriod;
        existing.CustomerNumber = cmd.CustomerNumber;
        existing.PaymentTerms   = cmd.PaymentTerms;
        existing.IntroText      = cmd.IntroText;
        existing.OutroText      = cmd.OutroText;
        existing.IsPaid         = cmd.IsPaid;
        existing.TotalNet       = cmd.TotalNet;
        existing.TotalTaxAmount = cmd.TotalTaxAmount;
        existing.TotalGross     = cmd.TotalGross;
        existing.Recipient      = recipient;
        existing.Provider       = provider;
        existing.Products       = cmd.Products.Select(p => new Product
        {
            ArticleNumber   = p.ArticleNumber,
            Name            = p.Name,
            Position        = p.Position,
            Quantity        = p.Quantity,
            Unit            = p.Unit,
            Description     = p.Description,
            TaxRate         = p.TaxRate,
            TaxAmount       = p.TaxAmount,
            UnitPrice       = p.UnitPrice,
            DiscountPercent = p.DiscountPercent,
            DiscountAmount  = p.DiscountAmount,
            TotalNet        = p.TotalNet,
            TotalGross      = p.TotalGross
        }).ToList();

        await invoiceRepository.UpdateAsync(existing, cancellationToken);
        await notificationService.NotifyInvoiceUpdatedAsync(cmd.UserId, existing.Id, existing.InvoiceNumber);
    }

    private static DateTime NormalizeUtc(DateTime dt) => dt.Kind switch
    {
        DateTimeKind.Utc   => dt,
        DateTimeKind.Local => dt.ToUniversalTime(),
        _                  => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
    };
}

// ─────────────────────────────────────────────────────────────────────────────

internal sealed class DeleteInvoiceHandler(
    IInvoiceRepository invoiceRepository,
    NotificationService notificationService)
    : ICommandHandler<DeleteInvoiceCommand>
{
    public async Task HandleAsync(DeleteInvoiceCommand cmd, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(cmd.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Rechnung {cmd.Id} nicht gefunden.");

        await invoiceRepository.DeleteAsync(cmd.Id, cancellationToken);
        await notificationService.NotifyInvoiceDeletedAsync(cmd.UserId, invoice.InvoiceNumber);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

internal sealed class MarkInvoicePaidHandler(
    IInvoiceRepository invoiceRepository)
    : ICommandHandler<MarkInvoicePaidCommand>
{
    public async Task HandleAsync(MarkInvoicePaidCommand cmd, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(cmd.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Rechnung {cmd.Id} nicht gefunden.");

        invoice.IsPaid = cmd.IsPaid;
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

internal sealed class ConvertOfferToInvoiceHandler(
    IOfferRepository offerRepository,
    IInvoiceRepository invoiceRepository,
    ISettingsRepository settingsRepository,
    NotificationService notificationService)
    : ICommandHandler<ConvertOfferToInvoiceCommand, int>
{
    public async Task<int> HandleAsync(ConvertOfferToInvoiceCommand cmd, CancellationToken cancellationToken = default)
    {
        var offer = await offerRepository.GetByIdAsync(cmd.OfferId, cancellationToken)
            ?? throw new KeyNotFoundException($"Angebot {cmd.OfferId} nicht gefunden.");

        var number = await GenerateNextNumberAsync(cmd.UserId, cancellationToken);

        var invoice = new Invoice
        {
            InvoiceNumber  = number,
            InvoiceDate    = DateTime.UtcNow,
            DueDate        = DateTime.UtcNow.AddDays(14),
            ServicePeriod  = offer.OfferDate.ToString("yyyy-MM"),
            CustomerNumber = offer.CustomerNumber,
            PaymentTerms   = "14 Tage netto",
            IntroText      = offer.IntroText,
            OutroText      = offer.OutroText,
            IsPaid         = false,
            Recipient = new Recipient
            {
                Name    = offer.Recipient.Name,
                Street  = offer.Recipient.Street,
                ZipCode = offer.Recipient.ZipCode,
                City    = offer.Recipient.City,
                Phone   = offer.Recipient.Phone,
                Email   = offer.Recipient.Email
            },
            Provider = new Provider
            {
                Name               = offer.Provider.Name,
                Company            = offer.Provider.Company,
                Street             = offer.Provider.Street,
                ZipCode            = offer.Provider.ZipCode,
                City               = offer.Provider.City,
                Phone              = offer.Provider.Phone,
                Email              = offer.Provider.Email,
                TaxId              = offer.Provider.TaxId,
                TaxNumber          = offer.Provider.TaxNumber,
                CommercialRegister = offer.Provider.CommercialRegister,
                RegisterCourt      = offer.Provider.RegisterCourt,
                Bank = new Bank
                {
                    Iban = offer.Provider.Bank.Iban,
                    Bic  = offer.Provider.Bank.Bic,
                    Name = offer.Provider.Bank.Name,
                    Plz  = offer.Provider.Bank.Plz,
                    Ort  = offer.Provider.Bank.Ort
                }
            },
            Products = cmd.IncludeOfferLines
                ? offer.Products.Select(p => new Product
                {
                    ArticleNumber   = p.ArticleNumber,
                    Name            = p.Name,
                    Position        = p.Position,
                    Quantity        = p.Quantity,
                    Unit            = p.Unit,
                    Description     = p.Description,
                    TaxRate         = p.TaxRate,
                    TaxAmount       = p.TaxAmount,
                    UnitPrice       = p.UnitPrice,
                    DiscountPercent = p.DiscountPercent,
                    DiscountAmount  = p.DiscountAmount,
                    TotalNet        = p.TotalNet,
                    TotalGross      = p.TotalGross
                }).ToList()
                : [],
            TotalNet       = cmd.IncludeOfferLines ? offer.TotalNet       : 0,
            TotalTaxAmount = cmd.IncludeOfferLines ? offer.TotalTaxAmount : 0,
            TotalGross     = cmd.IncludeOfferLines ? offer.TotalGross     : 0,
        };

        var created = await invoiceRepository.AddAsync(invoice, cancellationToken);
        await notificationService.NotifyInvoiceCreatedAsync(cmd.UserId, created.Id, created.InvoiceNumber);

        return created.Id;
    }

    private async Task<string> GenerateNextNumberAsync(string userId, CancellationToken ct)
    {
        var settings = await settingsRepository.GetSettingsAsync(userId, ct);
        var year      = DateTime.UtcNow.Year;
        var number    = $"{settings.InvoicePrefix}{year}-{settings.NextInvoiceNumber:D4}";
        settings.NextInvoiceNumber++;
        await settingsRepository.UpdateSettingsAsync(settings, ct);
        return number;
    }
}

