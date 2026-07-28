using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

/// <summary>
/// Material-Entnahme für einen Auftrag
/// </summary>
public record OrderMaterial
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// Geplante Menge (aus Angebot/Planung)
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal PlannedQuantity { get; set; }

    /// <summary>
    /// Tatsächlich entnommene Menge
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal ActualQuantity { get; set; }

    /// <summary>
    /// Wurde die Menge im Lager reserviert?
    /// </summary>
    public bool IsReserved { get; set; }

    /// <summary>
    /// Wurde die Entnahme bestätigt?
    /// </summary>
    public bool IsConfirmed { get; set; }

    public DateTime? ReservedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
