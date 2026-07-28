using System.ComponentModel.DataAnnotations;

namespace Handwerker.ApiService.Controllers;

// ── Response-DTOs ─────────────────────────────────────────────────────────────

public record WageTypeDto
{
    public int Id               { get; init; }
    public string WageNumber    { get; init; } = string.Empty;
    public string Name          { get; init; } = string.Empty;
    public string? Description  { get; init; }
    public decimal HourlyRate   { get; init; }
    public decimal TaxRate      { get; init; }
    public bool IsActive        { get; init; }
}

// ── Request-Typen ─────────────────────────────────────────────────────────────

public record CreateWageTypeRequest
{
    [Required, MaxLength(255)]
    public string Name          { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description  { get; init; }

    [Range(0, double.MaxValue)]
    public decimal HourlyRate   { get; init; }

    [Range(0, 100)]
    public decimal TaxRate      { get; init; } = 19;
}

public record UpdateWageTypeRequest
{
    public int Id                { get; init; }

    [Required, MaxLength(255)]
    public string Name           { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description   { get; init; }

    [Range(0, double.MaxValue)]
    public decimal HourlyRate    { get; init; }

    [Range(0, 100)]
    public decimal TaxRate       { get; init; }

    public bool IsActive         { get; init; }
}
