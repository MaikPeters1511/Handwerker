using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

public record Product
{
    public int Id { get; set; }
    
    // Referenz auf einen Artikelstamm (optional)
    [MaxLength(100)]
    public string ArticleNumber { get; set; } = string.Empty; 
    
    // Titel der Position (z.B. "Dachlatten 4x6")
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty; 

    [Range(1, int.MaxValue)]
    public int Position { get; set; } // Position
    
    [Range(0.01, double.MaxValue)]
    public double Quantity { get; set; } // Anzahl
    
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty; // Einheit
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty; // Beschreibung
    
    [Range(0, 100)]
    public decimal TaxRate { get; set; } // MwSt % (besser PascalCase)
    
    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; } // Errechnete MwSt Summe für diese Position

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; } // Einzelbetrag (Netto)
    
    // Rabatt auf Positionsebene
    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }
    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    // Gesamtsummen explizit machen
    [DataType(DataType.Currency)]
    public decimal TotalNet { get; set; } // Gesamt Netto
    [DataType(DataType.Currency)]
    public decimal TotalGross { get; set; } // Gesamt Brutto
}