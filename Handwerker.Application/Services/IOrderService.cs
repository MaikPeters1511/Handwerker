using Handwerker.Domain.Entities;

namespace Handwerker.Application.Services;

public interface IOrderService
{
    // CRUD
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, string createdBy, CancellationToken cancellationToken = default);
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Mehrere Angebote zu einem Auftrag erstellen
    Task<Order> CreateFromOffersAsync(
        List<int> offerIds,
        Order orderData,
        string createdBy,
        CancellationToken cancellationToken = default);

    // In Rechnung umwandeln
    Task<Invoice> ConvertToInvoiceAsync(
        int orderId,
        string createdBy,
        CancellationToken cancellationToken = default);

    // Status ändern
    Task<Order> UpdateStatusAsync(
        int orderId,
        OrderStatus newStatus,
        string userId,
        CancellationToken cancellationToken = default);

    // Arbeitszeiten
    Task<WorkTimeEntry> AddWorkTimeEntryAsync(
        int orderId,
        WorkTimeEntry entry,
        CancellationToken cancellationToken = default);

    Task<decimal> CalculateTotalHoursAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    // Material
    Task<OrderMaterial> AddMaterialAsync(
        int orderId,
        int articleId,
        int warehouseId,
        decimal plannedQuantity,
        CancellationToken cancellationToken = default);

    Task<OrderMaterial> ConfirmMaterialUsageAsync(
        int orderMaterialId,
        decimal actualQuantity,
        CancellationToken cancellationToken = default);

    // Suche und Filter
    Task<IEnumerable<Order>> SearchAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Order>> GetByCustomerNumberAsync(
        string customerNumber,
        CancellationToken cancellationToken = default);

    // Arbeitszeiten (Lesezugriff — Controller sollen IOrderService nutzen, nicht IOrderRepository direkt)
    Task<IEnumerable<WorkTimeEntry>> GetWorkTimeEntriesAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    Task DeleteWorkTimeEntryAsync(int entryId, CancellationToken cancellationToken = default);

    // Materialien (Lesezugriff)
    Task<IEnumerable<OrderMaterial>> GetOrderMaterialsAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}
