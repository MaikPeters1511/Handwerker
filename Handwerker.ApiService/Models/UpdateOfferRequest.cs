using Handwerker.Domain.Entities;

namespace Handwerker.ApiService.Models;

public record UpdateOfferRequest
{
    public int Id { get; set; }
    public string OfferNumber { get; set; } = string.Empty;
    public DateTime OfferDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public Recipient Recipient { get; set; } = new();
    public Provider Provider { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public decimal TotalNet { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalGross { get; set; }
    public OfferStatus Status { get; set; }
    public string IntroText { get; set; } = string.Empty;
    public string OutroText { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsReceived { get; set; }
    public int? ConvertedToOrderId { get; set; }
}