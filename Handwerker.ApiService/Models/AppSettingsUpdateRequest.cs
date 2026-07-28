namespace Handwerker.ApiService.Models;

public class AppSettingsUpdateRequest
{
    public required string Theme { get; init; }
    public required string LanguageCode { get; init; }
    public required bool EmailNotifications { get; init; }
    public required bool PushNotifications { get; init; }
    public required bool SmsNotifications { get; init; }
    public required string InvoicePrefix { get; init; }
    public required int NextInvoiceNumber { get; init; }
    public required decimal TaxRate { get; init; }
    public required string Currency { get; init; }
}