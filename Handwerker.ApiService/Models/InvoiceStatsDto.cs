namespace Handwerker.ApiService.Models;

public class InvoiceStatsDto
{
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int UnpaidInvoices { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal UnpaidAmount { get; set; }
}

