namespace Handwerker.ApiService.Models;

public class MonthlyStatsDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Invoices { get; set; }
    public int Offers { get; set; }
}

