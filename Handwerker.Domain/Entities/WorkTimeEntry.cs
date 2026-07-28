using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

/// <summary>
/// Arbeitszeiteintrag für Aufträge
/// </summary>
public record WorkTimeEntry
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public TimeSpan BreakDuration { get; set; }

    /// <summary>
    /// Berechnete Gesamtstunden (End - Start - Break)
    /// </summary>
    public TimeSpan TotalHours => EndTime - StartTime - BreakDuration;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsBillable { get; set; } = true;

    [DataType(DataType.Currency)]
    public decimal? HourlyRate { get; set; }

    // Multi-User Support
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
