using Handwerker.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[Route("api/[controller]")]
[Authorize]
public class InventoryController(IInventoryService inventoryService) : ApiControllerBase
{
    /// <summary>Wareneingang buchen</summary>
    [HttpPost("in")]
    public async Task<IActionResult> AddStock([FromBody] StockMovementRequest request, CancellationToken cancellationToken = default)
    {
        var movement = await inventoryService.AddStockAsync(
            request.ArticleId, request.WarehouseId, request.Quantity,
            request.Reason, GetUserId(), cancellationToken);
        return Ok(movement);
    }

    /// <summary>Warenausgang buchen</summary>
    [HttpPost("out")]
    public async Task<IActionResult> RemoveStock([FromBody] StockMovementRequest request, CancellationToken cancellationToken = default)
    {
        var movement = await inventoryService.RemoveStockAsync(
            request.ArticleId, request.WarehouseId, request.Quantity,
            request.Reason, GetUserId(), cancellationToken: cancellationToken);
        return Ok(movement);
    }

    /// <summary>Bestand reservieren (für Auftrag)</summary>
    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveStock([FromBody] ReserveStockRequest request, CancellationToken cancellationToken = default)
    {
        var movement = await inventoryService.ReserveStockAsync(
            request.ArticleId, request.WarehouseId, request.Quantity,
            request.Reason, GetUserId(), request.OrderId, cancellationToken);
        return Ok(movement);
    }

    /// <summary>Reservierung bestätigen</summary>
    [HttpPost("confirm-reservation/{movementId:int}")]
    public async Task<IActionResult> ConfirmReservation(int movementId, CancellationToken cancellationToken = default)
    {
        var movement = await inventoryService.ConfirmReservationAsync(movementId, GetUserId(), cancellationToken);
        return Ok(movement);
    }

    /// <summary>Reservierung stornieren</summary>
    [HttpPost("cancel-reservation/{movementId:int}")]
    public async Task<IActionResult> CancelReservation(int movementId, [FromBody] CancelReservationRequest request, CancellationToken cancellationToken = default)
    {
        var movement = await inventoryService.CancelReservationAsync(movementId, request.Reason, GetUserId(), cancellationToken);
        return Ok(movement);
    }

    /// <summary>Bestandskorrektur</summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequest request, CancellationToken cancellationToken = default)
    {
        var movement = await inventoryService.AdjustStockAsync(
            request.ArticleId, request.WarehouseId, request.NewQuantity,
            request.Reason, GetUserId(), cancellationToken);
        return Ok(movement);
    }

    /// <summary>Aktuellen Bestand abfragen</summary>
    [HttpGet("stock/{articleId:int}/{warehouseId:int}")]
    public async Task<IActionResult> GetStock(int articleId, int warehouseId, CancellationToken cancellationToken = default)
    {
        var stock     = await inventoryService.GetStockAsync(articleId, warehouseId, cancellationToken);
        var available = await inventoryService.GetAvailableStockAsync(articleId, warehouseId, cancellationToken);

        return Ok(new
        {
            ArticleId     = articleId,
            WarehouseId   = warehouseId,
            TotalStock    = stock,
            AvailableStock = available,
            ReservedStock = stock - available
        });
    }

    /// <summary>Verfügbarkeit prüfen</summary>
    [HttpGet("check/{articleId:int}/{warehouseId:int}")]
    public async Task<IActionResult> CheckAvailability(int articleId, int warehouseId, [FromQuery] decimal quantity, CancellationToken cancellationToken = default)
    {
        var available    = await inventoryService.HasSufficientStockAsync(articleId, warehouseId, quantity, cancellationToken);
        var currentStock = await inventoryService.GetAvailableStockAsync(articleId, warehouseId, cancellationToken);

        return Ok(new
        {
            Available         = available,
            RequestedQuantity = quantity,
            CurrentStock      = currentStock,
            Shortage          = available ? 0 : quantity - currentStock
        });
    }

    /// <summary>Lagerbewegungen eines Artikels abrufen</summary>
    [HttpGet("movements/{articleId:int}")]
    public async Task<IActionResult> GetMovements(int articleId, CancellationToken cancellationToken = default)
    {
        var movements = await inventoryService.GetMovementsAsync(articleId, cancellationToken);
        return Ok(movements);
    }

    /// <summary>Artikel mit niedrigem Bestand abrufen</summary>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockArticles(CancellationToken cancellationToken = default)
    {
        var articles = await inventoryService.GetLowStockArticlesAsync(cancellationToken);
        return Ok(articles);
    }
}
