using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

public class InventoryService(
    IArticleRepository articleRepository,
    IWarehouseRepository warehouseRepository) : IInventoryService
{
    // Wir brauchen einen Weg, um InventoryMovements zu speichern
    // Da wir keinen Zugriff auf DbContext haben, müssen wir ein Repository dafür erstellen
    // Für jetzt werfen wir NotImplementedException oder implementieren es später

    public async Task<InventoryMovement> AddStockAsync(
        int articleId,
        int warehouseId,
        decimal quantity,
        string reason,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Menge muss größer als 0 sein.");

        var articleWarehouse = await warehouseRepository.GetStockAsync(articleId, warehouseId, cancellationToken)
            ?? new ArticleWarehouse
            {
                ArticleId = articleId,
                WarehouseId = warehouseId,
                StockQuantity = 0,
                MinStockLevel = 0
            };

        var stockBefore = articleWarehouse.StockQuantity;
        articleWarehouse.StockQuantity += quantity;

        await warehouseRepository.UpdateStockAsync(articleWarehouse, cancellationToken);

        // TODO: InventoryMovement speichern
        // Da wir keinen Zugriff auf DbContext haben, müssen wir das über ein Repository machen
        return new InventoryMovement
        {
            ArticleId = articleId,
            WarehouseId = warehouseId,
            Type = MovementType.In,
            Quantity = quantity,
            StockBefore = stockBefore,
            StockAfter = articleWarehouse.StockQuantity,
            ReferenceType = "Manual",
            Reason = reason,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<InventoryMovement> RemoveStockAsync(
        int articleId,
        int warehouseId,
        decimal quantity,
        string reason,
        string createdBy,
        string referenceType = "Manual",
        int? referenceId = null,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Menge muss größer als 0 sein.");

        var articleWarehouse = await warehouseRepository.GetStockAsync(articleId, warehouseId, cancellationToken);
        if (articleWarehouse == null || articleWarehouse.StockQuantity < quantity)
            throw new InvalidOperationException("Nicht ausreichend Bestand vorhanden.");

        var stockBefore = articleWarehouse.StockQuantity;
        articleWarehouse.StockQuantity -= quantity;

        await warehouseRepository.UpdateStockAsync(articleWarehouse, cancellationToken);

        // TODO: InventoryMovement speichern
        return new InventoryMovement
        {
            ArticleId = articleId,
            WarehouseId = warehouseId,
            Type = MovementType.Out,
            Quantity = quantity,
            StockBefore = stockBefore,
            StockAfter = articleWarehouse.StockQuantity,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Reason = reason,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Task<InventoryMovement> ReserveStockAsync(
        int articleId,
        int warehouseId,
        decimal quantity,
        string reason,
        string createdBy,
        int orderId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implementieren wenn IInventoryMovementRepository existiert
        throw new NotImplementedException("Reservierung wird in Phase 2 implementiert");
    }

    public Task<InventoryMovement> ConfirmReservationAsync(
        int movementId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implementieren wenn IInventoryMovementRepository existiert
        throw new NotImplementedException("Reservierungsbestätigung wird in Phase 2 implementiert");
    }

    public Task<InventoryMovement> CancelReservationAsync(
        int movementId,
        string reason,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implementieren wenn IInventoryMovementRepository existiert
        throw new NotImplementedException("Reservierungsstornierung wird in Phase 2 implementiert");
    }

    public async Task<InventoryMovement> AdjustStockAsync(
        int articleId,
        int warehouseId,
        decimal newQuantity,
        string reason,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        if (newQuantity < 0)
            throw new InvalidOperationException("Bestand kann nicht negativ sein.");

        var articleWarehouse = await warehouseRepository.GetStockAsync(articleId, warehouseId, cancellationToken)
            ?? new ArticleWarehouse
            {
                ArticleId = articleId,
                WarehouseId = warehouseId,
                StockQuantity = 0,
                MinStockLevel = 0
            };

        var stockBefore = articleWarehouse.StockQuantity;
        articleWarehouse.StockQuantity = newQuantity;

        await warehouseRepository.UpdateStockAsync(articleWarehouse, cancellationToken);

        // TODO: InventoryMovement speichern
        return new InventoryMovement
        {
            ArticleId = articleId,
            WarehouseId = warehouseId,
            Type = MovementType.Adjustment,
            Quantity = Math.Abs(newQuantity - stockBefore),
            StockBefore = stockBefore,
            StockAfter = newQuantity,
            ReferenceType = "Manual",
            Reason = reason,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<decimal> GetStockAsync(
        int articleId,
        int warehouseId,
        CancellationToken cancellationToken = default)
    {
        var articleWarehouse = await warehouseRepository.GetStockAsync(articleId, warehouseId, cancellationToken);
        return articleWarehouse?.StockQuantity ?? 0;
    }

    public async Task<decimal> GetAvailableStockAsync(
        int articleId,
        int warehouseId,
        CancellationToken cancellationToken = default)
    {
        // Ohne InventoryMovement-Repository können wir Reservierungen nicht tracken
        // Rückfall: Gesamtbestand zurückgeben
        return await GetStockAsync(articleId, warehouseId, cancellationToken);
    }

    public Task<IEnumerable<InventoryMovement>> GetMovementsAsync(
        int articleId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implementieren wenn IInventoryMovementRepository existiert
        return Task.FromResult<IEnumerable<InventoryMovement>>(new List<InventoryMovement>());
    }

    public async Task<bool> HasSufficientStockAsync(
        int articleId,
        int warehouseId,
        decimal requiredQuantity,
        CancellationToken cancellationToken = default)
    {
        var available = await GetAvailableStockAsync(articleId, warehouseId, cancellationToken);
        return available >= requiredQuantity;
    }

    public async Task<IEnumerable<Article>> GetLowStockArticlesAsync(
        CancellationToken cancellationToken = default)
    {
        return await articleRepository.GetLowStockAsync(cancellationToken);
    }
}
