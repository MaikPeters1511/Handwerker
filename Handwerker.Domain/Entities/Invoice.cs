using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

public record Invoice
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string InvoiceNumber { get; set; } = string.Empty; // Rechnungsnummer

    [DataType(DataType.Date)]
    public DateTime InvoiceDate { get; set; } // Rechnungsdatum

    [MaxLength(100)]
    public string ServicePeriod { get; set; } = string.Empty; // Leistungsdatum/-zeitraum

    [MaxLength(30)]
    public string CustomerNumber { get; set; } = string.Empty; // Kundennummer
    
    // Der Rechnungsempfänger (Wichtig: Als Kopie/Snapshot speichern!)
    public Recipient Recipient { get; set; } = new();

    // Details of the service provider (IHR HANDWERKER)
    public Provider Provider { get; set; } = new();

    // Invoice line items
    public List<Product> Products { get; set; } = new List<Product>();

    [DataType(DataType.Currency)]
    // Financial totals
    public decimal TotalNet { get; set; }        // Summe Netto über alle Positionen
    
    [DataType(DataType.Currency)]
    public decimal TotalTaxAmount { get; set; }  // Summe MwSt.
    
    [DataType(DataType.Currency)]
    public decimal TotalGross { get; set; }      // Endbetrag (Brutto)

    // Zahlungsmodalitäten & Status
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }        // Fälligkeitsdatum
    
    [MaxLength(100)]
    public string PaymentTerms { get; set; } = string.Empty; // z.B. "Zahlbar innerhalb 14 Tagen"
    
    public bool IsPaid { get; set; }             // Bezahlstatus

    // Textelemente
    [MaxLength(2000)]
    public string IntroText { get; set; } = string.Empty; // Text oben (z.B. "Vielen Dank für Ihren Auftrag...")
    
    [MaxLength(2000)]
    public string OutroText { get; set; } = string.Empty; // Text unten
}