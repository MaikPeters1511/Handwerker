using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

/// <summary>
/// Lagerort (z.B. Hauptlager, Halle A, Baustelle X)
/// </summary>
public record Warehouse
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public List<ArticleWarehouse> ArticleWarehouses { get; set; } = new();
}
