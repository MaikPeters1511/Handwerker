using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

/// <summary>
/// Verknüpfungstabelle für Artikelbestand pro Lager
/// </summary>
public record ArticleWarehouse
{
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal StockQuantity { get; set; } = 0;

    [Range(0, double.MaxValue)]
    public decimal MinStockLevel { get; set; } = 0;

    [Range(0, double.MaxValue)]
    public decimal? MaxStockLevel { get; set; }

    [MaxLength(100)]
    public string? StorageLocation { get; set; } // z.B. "Regal 3, Fach B"

    public bool IsLowStock => StockQuantity < MinStockLevel;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
