using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

/// <summary>
/// Lagerbewegung für Audit-Trail
/// </summary>
public record InventoryMovement
{
    public int Id { get; set; }

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public MovementType Type { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StockBefore { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StockAfter { get; set; }

    /// <summary>
    /// Referenztyp: Order, Manual, Initial, Migration
    /// </summary>
    [MaxLength(50)]
    public string ReferenceType { get; set; } = string.Empty;

    /// <summary>
    /// Referenz-ID (z.B. Order-ID)
    /// </summary>
    public int? ReferenceId { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
