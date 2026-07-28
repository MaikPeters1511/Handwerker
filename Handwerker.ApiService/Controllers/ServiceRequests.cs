using System.ComponentModel.DataAnnotations;

namespace Handwerker.ApiService.Controllers;

// ── Response-DTOs ─────────────────────────────────────────────────────────────

public record ServiceItemDto
{
    public int Id                { get; init; }
    public string ServiceNumber  { get; init; } = string.Empty;
    public string Name           { get; init; } = string.Empty;
    public string? Description   { get; init; }
    public string Unit           { get; init; } = string.Empty;
    public decimal UnitPrice     { get; init; }
    public decimal TaxRate       { get; init; }
    public bool IsActive         { get; init; }
}

// ── Request-Typen ─────────────────────────────────────────────────────────────

public record CreateServiceItemRequest
{
    [Required, MaxLength(255)]
    public string Name          { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description  { get; init; }

    [Required, MaxLength(50)]
    public string Unit          { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal UnitPrice    { get; init; }

    [Range(0, 100)]
    public decimal TaxRate      { get; init; } = 19;
}

public record UpdateServiceItemRequest
{
    public int Id                { get; init; }

    [Required, MaxLength(255)]
    public string Name           { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description   { get; init; }

    [Required, MaxLength(50)]
    public string Unit           { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal UnitPrice     { get; init; }

    [Range(0, 100)]
    public decimal TaxRate       { get; init; }

    public bool IsActive         { get; init; }
}
