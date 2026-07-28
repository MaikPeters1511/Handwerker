using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

public interface IInventoryService
{
    /// <summary>
    /// Wareneingang buchen
    /// </summary>
    Task<InventoryMovement> AddStockAsync(
        int articleId,
        int warehouseId,
        decimal quantity,
        string reason,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Warenausgang buchen
    /// </summary>
    Task<InventoryMovement> RemoveStockAsync(
        int articleId,
        int warehouseId,
        decimal quantity,
        string reason,
        string createdBy,
        string referenceType = "Manual",
        int? referenceId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bestand reservieren (für Auftrag)
    /// </summary>
    Task<InventoryMovement> ReserveStockAsync(
        int articleId,
        int warehouseId,
        decimal quantity,
        string reason,
        string createdBy,
        int orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reservierung bestätigen (wird zu Ausgang)
    /// </summary>
    Task<InventoryMovement> ConfirmReservationAsync(
        int movementId,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reservierung stornieren
    /// </summary>
    Task<InventoryMovement> CancelReservationAsync(
        int movementId,
        string reason,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bestandskorrektur durchführen
    /// </summary>
    Task<InventoryMovement> AdjustStockAsync(
        int articleId,
        int warehouseId,
        decimal newQuantity,
        string reason,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktuellen Bestand abfragen
    /// </summary>
    Task<decimal> GetStockAsync(
        int articleId,
        int warehouseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verfügbaren Bestand abfragen (ohne Reservierungen)
    /// </summary>
    Task<decimal> GetAvailableStockAsync(
        int articleId,
        int warehouseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Alle Bewegungen eines Artikels abrufen
    /// </summary>
    Task<IEnumerable<InventoryMovement>> GetMovementsAsync(
        int articleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prüft ob ausreichend Bestand vorhanden ist
    /// </summary>
    Task<bool> HasSufficientStockAsync(
        int articleId,
        int warehouseId,
        decimal requiredQuantity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Artikel mit niedrigem Bestand abrufen
    /// </summary>
    Task<IEnumerable<Article>> GetLowStockArticlesAsync(
        CancellationToken cancellationToken = default);
}
