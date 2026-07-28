using System.ComponentModel.DataAnnotations;
namespace Handwerker.Domain.Entities;

public record AppSettings
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Theme { get; set; } = "light";

    [MaxLength(10)]
    public string LanguageCode { get; set; } = "de";

    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = false;
    public bool SmsNotifications { get; set; } = false;

    [MaxLength(20)]
    public string InvoicePrefix { get; set; } = "RE-";

    public int NextInvoiceNumber { get; set; } = 1001;

    public decimal TaxRate { get; set; } = 19m;

    [MaxLength(10)]
    public string Currency { get; set; } = "EUR";
}
