namespace Handwerker.ApiService.Controllers;

// ── Response-DTOs ─────────────────────────────────────────────────────────────

public record ArticleDto
{
    public int Id                   { get; init; }
    public string ArticleNumber     { get; init; } = string.Empty;
    public string Name              { get; init; } = string.Empty;
    public string? Description      { get; init; }
    public string Unit              { get; init; } = string.Empty;
    public decimal UnitPrice        { get; init; }
    public decimal TaxRate          { get; init; }
    public string? Category         { get; init; }
    public bool IsActive            { get; init; }
    public decimal TotalStock       { get; init; }
}

public record ArticleDetailDto : ArticleDto
{
    public List<ArticleWarehouseDto> WarehouseStocks { get; init; } = [];
}

public record ArticleWarehouseDto
{
    public int WarehouseId          { get; init; }
    public string WarehouseName     { get; init; } = string.Empty;
    public decimal StockQuantity    { get; init; }
    public decimal MinStockLevel    { get; init; }
    public decimal? MaxStockLevel   { get; init; }
    public string? StorageLocation  { get; init; }
    public bool IsLowStock          { get; init; }
}

// ── Request-Typen ─────────────────────────────────────────────────────────────

public record CreateArticleRequest
{
    public string ArticleNumber  { get; init; } = string.Empty;
    public string Name           { get; init; } = string.Empty;
    public string? Description   { get; init; }
    public string Unit           { get; init; } = string.Empty;
    public decimal UnitPrice     { get; init; }
    public decimal TaxRate       { get; init; } = 19;
    public string? Category      { get; init; }
}

public record UpdateArticleRequest
{
    public int Id                { get; init; }
    public string ArticleNumber  { get; init; } = string.Empty;
    public string Name           { get; init; } = string.Empty;
    public string? Description   { get; init; }
    public string Unit           { get; init; } = string.Empty;
    public decimal UnitPrice     { get; init; }
    public decimal TaxRate       { get; init; }
    public string? Category      { get; init; }
    public bool IsActive         { get; init; }
}
