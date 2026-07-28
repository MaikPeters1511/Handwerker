namespace Handwerker.Domain.Entities;

/// <summary>
/// Verknüpfung zwischen Order und Offer (mehrere Angebote können zu einem Auftrag führen)
/// </summary>
public record OrderOffer
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int OfferId { get; set; }
    public Offer Offer { get; set; } = null!;

    /// <summary>
    /// Prozentualer Anteil dieses Angebots am Auftrag (z.B. 50% wenn 2 Angebote zusammengeführt)
    /// </summary>
    public decimal PortionPercentage { get; set; } = 100;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
