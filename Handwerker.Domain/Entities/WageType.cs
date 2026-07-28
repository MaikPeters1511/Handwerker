// Handwerker.Domain/Entities/WageType.cs
using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

/// <summary>
/// Lohnartenstamm für Personalkosten-Sätze (z.B. Facharbeiter, Meister, Azubi).
/// Enthält keinen Bezug zu konkreten Mitarbeitern (kein personenbezogenes Datum).
/// </summary>
public record WageType
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string WageNumber { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal HourlyRate { get; set; }

    [Range(0, 100)]
    public decimal TaxRate { get; set; } = 19;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
