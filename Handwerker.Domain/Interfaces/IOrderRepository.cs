using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerNumberAsync(string customerNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    // WorkTimeEntry methods
    Task<WorkTimeEntry> AddWorkTimeEntryAsync(WorkTimeEntry entry, CancellationToken cancellationToken = default);
    Task UpdateWorkTimeEntryAsync(WorkTimeEntry entry, CancellationToken cancellationToken = default);
    Task DeleteWorkTimeEntryAsync(int entryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkTimeEntry>> GetWorkTimeEntriesAsync(int orderId, CancellationToken cancellationToken = default);

    // OrderMaterial methods
    Task<OrderMaterial> AddOrderMaterialAsync(OrderMaterial material, CancellationToken cancellationToken = default);
    Task UpdateOrderMaterialAsync(OrderMaterial material, CancellationToken cancellationToken = default);
    Task DeleteOrderMaterialAsync(int materialId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderMaterial>> GetOrderMaterialsAsync(int orderId, CancellationToken cancellationToken = default);

    // OrderOffer methods
    Task AddOrderOfferAsync(OrderOffer orderOffer, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderOffer>> GetOrderOffersAsync(int orderId, CancellationToken cancellationToken = default);
}
