namespace Handwerker.ApiService.Controllers;

// Inventory-Anfrage-Typen (API-Boundary)

public record StockMovementRequest
{
    public int ArticleId    { get; init; }
    public int WarehouseId  { get; init; }
    public decimal Quantity { get; init; }
    public string Reason    { get; init; } = string.Empty;
}

public record ReserveStockRequest
{
    public int ArticleId    { get; init; }
    public int WarehouseId  { get; init; }
    public decimal Quantity { get; init; }
    public string Reason    { get; init; } = string.Empty;
    public int OrderId      { get; init; }
}

public record CancelReservationRequest
{
    public string Reason { get; init; } = string.Empty;
}

public record AdjustStockRequest
{
    public int ArticleId       { get; init; }
    public int WarehouseId     { get; init; }
    public decimal NewQuantity { get; init; }
    public string Reason       { get; init; } = string.Empty;
}
