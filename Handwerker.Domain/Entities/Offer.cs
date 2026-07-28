using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

public record Offer
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string OfferNumber { get; set; } = string.Empty; // Angebotsnummer

    [DataType(DataType.Date)]
    public DateTime OfferDate { get; set; } // Angebotsdatum

    [DataType(DataType.Date)]
    public DateTime ValidUntil { get; set; } // Gültig bis

    [MaxLength(30)]
    public string CustomerNumber { get; set; } = string.Empty; // Kundennummer
    
    // Der Angebotsempfänger (Wichtig: Als Kopie/Snapshot speichern!)
    public Recipient Recipient { get; set; } = new();

    // Details of the service provider (IHR HANDWERKER)
    public Provider Provider { get; set; } = new();

    // Offer line items
    public List<Product> Products { get; set; } = new();

    [DataType(DataType.Currency)]
    // Financial totals
    public decimal TotalNet { get; set; }        // Summe Netto über alle Positionen
    
    [DataType(DataType.Currency)]
    public decimal TotalTaxAmount { get; set; }  // Summe MwSt.
    
    [DataType(DataType.Currency)]
    public decimal TotalGross { get; set; }      // Endbetrag (Brutto)

    // Status des Angebots
    public OfferStatus Status { get; set; } = OfferStatus.Draft;

    // Textelemente
    [MaxLength(2000)]
    public string IntroText { get; set; } = string.Empty; // Text oben (z.B. "Wir danken Ihnen für Ihre Anfrage...")
    
    [MaxLength(2000)]
    public string OutroText { get; set; } = string.Empty; // Text unten

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty; // Belegbemerkungen

    // Typ: Geschrieben oder Empfangen
    public bool IsReceived { get; set; } = false; // false = geschrieben, true = empfangen

    // Referenz zum konvertierten Auftrag (wenn umgewandelt)
    public int? ConvertedToOrderId { get; set; }
}
