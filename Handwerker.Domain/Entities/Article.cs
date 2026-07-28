using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

/// <summary>
/// Artikelstamm für Lagerhaltung und Materialentnahme
/// </summary>
public record Article
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string ArticleNumber { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty; // Stk, m, kg, etc.

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, 100)]
    public decimal TaxRate { get; set; } = 19;

    // Artikelkategorie
    [MaxLength(100)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public List<ArticleWarehouse> ArticleWarehouses { get; set; } = new();

    public List<InventoryMovement> InventoryMovements { get; set; } = new();

    // Berechneter Gesamtbestand über alle Lager
    public decimal TotalStock => ArticleWarehouses?.Sum(aw => aw.StockQuantity) ?? 0;
}
