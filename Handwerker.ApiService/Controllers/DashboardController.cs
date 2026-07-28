using Handwerker.Application.Services;
using Handwerker.ApiService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(
    ProductService productService,
    RecipientService recipientService,
    InvoiceService invoiceService,
    OfferService offerService) : ControllerBase
{
    /// <summary>
    /// Lädt die Dashboard-Statistiken für die Übersicht
    /// Zählt die Anzahl der Einträge in jeder Tabelle
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken = default)
    {
        // EF Core DbContext ist nicht thread-safe - sequenziell ausführen
        var productsCount = await productService.CountAsync(cancellationToken);
        var recipientsCount = await recipientService.CountAsync(cancellationToken);
        var invoicesCount = await invoiceService.CountAsync(cancellationToken);
        var offersCount = await offerService.CountAsync(cancellationToken);
        
        var stats = new DashboardStatsDto
        {
            Offers = offersCount,
            Invoices = invoicesCount,
            Products = productsCount,
            Recipients = recipientsCount
        };

        return Ok(stats);
    }

    /// <summary>
    /// Lädt die monatlichen Statistiken für die letzten 6 Monate
    /// Zählt Angebote und Rechnungen pro Monat
    /// </summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyStats(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;

        // Lade alle Invoices und Offers der letzten 6 Monate
        var invoices = (await invoiceService.GetAllAsync(cancellationToken)).ToList();
        var offers = (await offerService.GetAllAsync(cancellationToken)).ToList();

        // Filtere und gruppiere nach Monat
        var monthlyStats = new List<MonthlyStatsDto>();
        
        for (int i = 5; i >= 0; i--)
        {
            var targetDate = today.AddMonths(-i);
            var monthStart = new DateTime(targetDate.Year, targetDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var invoiceCount = invoices.Count(inv => 
                inv.InvoiceDate >= monthStart && inv.InvoiceDate <= monthEnd);
            
            var offerCount = offers.Count(off => 
                off.OfferDate >= monthStart && off.OfferDate <= monthEnd);

            monthlyStats.Add(new MonthlyStatsDto
            {
                Month = targetDate.Month.ToString(), // Monatsnummer für i18n
                Year = targetDate.Year,
                Invoices = invoiceCount,
                Offers = offerCount
            });
        }

        return Ok(monthlyStats);
    }

    /// <summary>
    /// Lädt die monatlichen Rechnungsbeträge für die letzten 6 Monate
    /// Summiert Brutto, Netto und MwSt. pro Monat
    /// </summary>
    [HttpGet("monthly-amounts")]
    public async Task<IActionResult> GetMonthlyAmounts(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;

        // Lade alle Invoices
        var invoices = (await invoiceService.GetAllAsync(cancellationToken)).ToList();

        // Filtere und gruppiere nach Monat
        var monthlyAmounts = new List<MonthlyAmountsDto>();
        
        for (int i = 5; i >= 0; i--)
        {
            var targetDate = today.AddMonths(-i);
            var monthStart = new DateTime(targetDate.Year, targetDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthInvoices = invoices.Where(inv => 
                inv.InvoiceDate >= monthStart && inv.InvoiceDate <= monthEnd).ToList();

            monthlyAmounts.Add(new MonthlyAmountsDto
            {
                Month = targetDate.Month.ToString(),
                Year = targetDate.Year,
                TotalGross = monthInvoices.Sum(inv => inv.TotalGross),
                TotalNet = monthInvoices.Sum(inv => inv.TotalNet),
                TotalTax = monthInvoices.Sum(inv => inv.TotalTaxAmount)
            });
        }

        return Ok(monthlyAmounts);
    }

    /// <summary>
    /// Lädt die Rechnungsstatistik (bezahlt/unbezahlt)
    /// </summary>
    [HttpGet("invoice-stats")]
    public async Task<IActionResult> GetInvoiceStats(CancellationToken cancellationToken = default)
    {
        var invoices = (await invoiceService.GetAllAsync(cancellationToken)).ToList();

        var paidInvoices = invoices.Where(inv => inv.IsPaid).ToList();
        var unpaidInvoices = invoices.Where(inv => !inv.IsPaid).ToList();

        var stats = new InvoiceStatsDto
        {
            TotalInvoices = invoices.Count,
            PaidInvoices = paidInvoices.Count,
            UnpaidInvoices = unpaidInvoices.Count,
            TotalAmount = invoices.Sum(inv => inv.TotalGross),
            PaidAmount = paidInvoices.Sum(inv => inv.TotalGross),
            UnpaidAmount = unpaidInvoices.Sum(inv => inv.TotalGross)
        };

        return Ok(stats);
    }
}
