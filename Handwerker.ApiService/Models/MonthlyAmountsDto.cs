namespace Handwerker.ApiService.Models;

public class MonthlyAmountsDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalTax { get; set; }
}

