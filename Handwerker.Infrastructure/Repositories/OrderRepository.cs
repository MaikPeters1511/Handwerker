using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class OrderRepository(HandwerkerDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.SourceOffers)
            .ThenInclude(oo => oo.Offer)
            .Include(o => o.Products)
            .Include(o => o.Materials)
            .ThenInclude(m => m.Article)
            .Include(o => o.WorkTimeEntries)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.SourceOffers)
            .ThenInclude(oo => oo.Offer)
            .Include(o => o.Products)
            .Include(o => o.Materials)
            .ThenInclude(m => m.Article)
            .Include(o => o.WorkTimeEntries)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.SourceOffers)
            .ThenInclude(oo => oo.Offer)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Where(o => o.Status == status)
            .Include(o => o.SourceOffers)
            .ThenInclude(oo => oo.Offer)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByCustomerNumberAsync(string customerNumber, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Where(o => o.CustomerNumber == customerNumber)
            .Include(o => o.SourceOffers)
            .ThenInclude(oo => oo.Offer)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await context.Orders
            .Where(o =>
                o.OrderNumber.ToLower().Contains(term) ||
                o.CustomerNumber.ToLower().Contains(term) ||
                o.Description.ToLower().Contains(term) ||
                o.Recipient.Name.ToLower().Contains(term))
            .Include(o => o.SourceOffers)
            .ThenInclude(oo => oo.Offer)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        order.UpdatedAt = DateTime.UtcNow;
        context.Orders.Update(order);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await context.Orders.FindAsync(new object[] { id }, cancellationToken);
        if (order != null)
        {
            // Bei Aufträgen wirklich löschen (kein Soft Delete, da komplexe Verknüpfungen)
            context.Orders.Remove(order);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await context.Orders.CountAsync(cancellationToken);
    }

    // WorkTimeEntry methods
    public async Task<WorkTimeEntry> AddWorkTimeEntryAsync(WorkTimeEntry entry, CancellationToken cancellationToken = default)
    {
        context.WorkTimeEntries.Add(entry);
        await context.SaveChangesAsync(cancellationToken);
        return entry;
    }

    public async Task UpdateWorkTimeEntryAsync(WorkTimeEntry entry, CancellationToken cancellationToken = default)
    {
        context.WorkTimeEntries.Update(entry);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteWorkTimeEntryAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var entry = await context.WorkTimeEntries.FindAsync(new object[] { entryId }, cancellationToken);
        if (entry != null)
        {
            context.WorkTimeEntries.Remove(entry);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<WorkTimeEntry>> GetWorkTimeEntriesAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await context.WorkTimeEntries
            .Where(w => w.OrderId == orderId)
            .OrderByDescending(w => w.Date)
            .ToListAsync(cancellationToken);
    }

    // OrderMaterial methods
    public async Task<OrderMaterial> AddOrderMaterialAsync(OrderMaterial material, CancellationToken cancellationToken = default)
    {
        context.OrderMaterials.Add(material);
        await context.SaveChangesAsync(cancellationToken);
        return material;
    }

    public async Task UpdateOrderMaterialAsync(OrderMaterial material, CancellationToken cancellationToken = default)
    {
        context.OrderMaterials.Update(material);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteOrderMaterialAsync(int materialId, CancellationToken cancellationToken = default)
    {
        var material = await context.OrderMaterials.FindAsync(new object[] { materialId }, cancellationToken);
        if (material != null)
        {
            context.OrderMaterials.Remove(material);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<OrderMaterial>> GetOrderMaterialsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await context.OrderMaterials
            .Where(m => m.OrderId == orderId)
            .Include(m => m.Article)
            .Include(m => m.Warehouse)
            .ToListAsync(cancellationToken);
    }

    // OrderOffer methods
    public async Task AddOrderOfferAsync(OrderOffer orderOffer, CancellationToken cancellationToken = default)
    {
        context.OrderOffers.Add(orderOffer);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderOffer>> GetOrderOffersAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await context.OrderOffers
            .Where(oo => oo.OrderId == orderId)
            .Include(oo => oo.Offer)
            .ToListAsync(cancellationToken);
    }
}
