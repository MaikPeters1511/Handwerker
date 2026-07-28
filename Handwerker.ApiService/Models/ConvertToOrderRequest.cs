namespace Handwerker.ApiService.Models;

// Request für Convert to Order
public record ConvertToOrderRequest
{
    public int OfferId { get; set; }
}
