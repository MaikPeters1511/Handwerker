using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

/// <summary>
/// Leistungsstamm für Dienstleistungen/Arbeitsleistungen (z.B. Montage, Beratung, Wartung).
/// </summary>
public record ServiceItem
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string ServiceNumber { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty; // Std., Pauschale, m², etc.

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, 100)]
    public decimal TaxRate { get; set; } = 19;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
