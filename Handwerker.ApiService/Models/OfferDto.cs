using Handwerker.Domain.Entities;

namespace Handwerker.ApiService.Models;

// DTO für Liste
public record OfferDto
{
    public int Id { get; set; }
    public string OfferNumber { get; set; } = string.Empty;
    public DateTime OfferDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty; // Von Recipient
    public decimal TotalNet { get; set; }
    public decimal TotalGross { get; set; }
    public OfferStatus Status { get; set; }
    public bool IsReceived { get; set; }
    public int? ConvertedToOrderId { get; set; }
}