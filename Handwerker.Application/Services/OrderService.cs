using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IOfferRepository offerRepository,
    IInvoiceRepository invoiceRepository) : IOrderService
{
    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await orderRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await orderRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Order> CreateAsync(Order order, string createdBy, CancellationToken cancellationToken = default)
    {
        // Auftragsnummer generieren
        order.OrderNumber = await GenerateOrderNumberAsync(cancellationToken);
        order.CreatedBy = createdBy;
        order.CreatedAt = DateTime.UtcNow;
        order.Status = OrderStatus.Draft;

        // Summen berechnen
        CalculateTotals(order);

        return await orderRepository.AddAsync(order, cancellationToken);
    }

    public async Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        // Summen neu berechnen
        CalculateTotals(order);

        await orderRepository.UpdateAsync(order, cancellationToken);
        return order;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await orderRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<Order> CreateFromOffersAsync(
        List<int> offerIds,
        Order orderData,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        if (offerIds == null || offerIds.Count == 0)
            throw new InvalidOperationException("Mindestens ein Angebot muss angegeben werden.");

        var offers = new List<Offer>();
        foreach (var offerId in offerIds)
        {
            var offer = await offerRepository.GetByIdAsync(offerId, cancellationToken);
            if (offer == null)
                throw new InvalidOperationException($"Angebot mit ID {offerId} nicht gefunden.");
            offers.Add(offer);
        }

        // Ersten Kunden als Basis verwenden
        var firstOffer = offers.First();
        orderData.CustomerNumber = firstOffer.CustomerNumber;
        orderData.Recipient = firstOffer.Recipient;
        orderData.Provider = firstOffer.Provider;

        // Produkte zusammenführen
        orderData.Products = new List<Product>();
        foreach (var offer in offers)
        {
            orderData.Products.AddRange(offer.Products);
        }

        // Auftrag erstellen
        var order = await CreateAsync(orderData, createdBy, cancellationToken);

        // Angebotsverknüpfungen erstellen
        var portionPercentage = 100m / offers.Count;
        foreach (var offer in offers)
        {
            await orderRepository.AddOrderOfferAsync(new OrderOffer
            {
                OrderId = order.Id,
                OfferId = offer.Id,
                PortionPercentage = portionPercentage
            }, cancellationToken);

            // Angebotsstatus aktualisieren
            offer.Status = OfferStatus.Converted;
            await offerRepository.UpdateAsync(offer, cancellationToken);
        }

        return order;
    }

    public async Task<Invoice> ConvertToInvoiceAsync(
        int orderId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException("Auftrag nicht gefunden.");

        if (order.Status != OrderStatus.Completed)
            throw new InvalidOperationException("Auftrag muss abgeschlossen sein, um in Rechnung gestellt zu werden.");

        // Neue Rechnung erstellen
        var invoice = new Invoice
        {
            InvoiceNumber = await GenerateInvoiceNumberAsync(cancellationToken),
            InvoiceDate = DateTime.UtcNow,
            CustomerNumber = order.CustomerNumber,
            Recipient = order.Recipient,
            Provider = order.Provider,
            Products = order.Products.Select(p => new Product
            {
                ArticleNumber = p.ArticleNumber,
                Name = p.Name,
                Position = p.Position,
                Quantity = p.Quantity,
                Unit = p.Unit,
                Description = p.Description,
                TaxRate = p.TaxRate,
                TaxAmount = p.TaxAmount,
                UnitPrice = p.UnitPrice,
                DiscountPercent = p.DiscountPercent,
                DiscountAmount = p.DiscountAmount,
                TotalNet = p.TotalNet,
                TotalGross = p.TotalGross
            }).ToList(),
            TotalNet = order.TotalNet,
            TotalTaxAmount = order.TotalTaxAmount,
            TotalGross = order.TotalGross,
            DueDate = DateTime.UtcNow.AddDays(14),
            PaymentTerms = "Zahlbar innerhalb 14 Tagen",
            IsPaid = false
        };

        var createdInvoice = await invoiceRepository.AddAsync(invoice, cancellationToken);

        // Auftrag aktualisieren
        order.InvoiceId = createdInvoice.Id;
        order.Status = OrderStatus.Invoiced;
        await orderRepository.UpdateAsync(order, cancellationToken);

        return createdInvoice;
    }

    public async Task<Order> UpdateStatusAsync(
        int orderId,
        OrderStatus newStatus,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException("Auftrag nicht gefunden.");

        // Status-Validierung
        ValidateStatusTransition(order.Status, newStatus);

        order.Status = newStatus;

        // Automatische Aktionen basierend auf Status
        switch (newStatus)
        {
            case OrderStatus.InProgress:
                order.ActualStartDate ??= DateTime.UtcNow;
                break;
            case OrderStatus.Completed:
                order.ActualEndDate = DateTime.UtcNow;
                break;
        }

        await orderRepository.UpdateAsync(order, cancellationToken);
        return order;
    }

    public async Task<WorkTimeEntry> AddWorkTimeEntryAsync(
        int orderId,
        WorkTimeEntry entry,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException("Auftrag nicht gefunden.");

        entry.OrderId = orderId;
        return await orderRepository.AddWorkTimeEntryAsync(entry, cancellationToken);
    }

    public async Task<decimal> CalculateTotalHoursAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var entries = await orderRepository.GetWorkTimeEntriesAsync(orderId, cancellationToken);
        return entries.Sum(e => (decimal)e.TotalHours.TotalHours);
    }

    public async Task<OrderMaterial> AddMaterialAsync(
        int orderId,
        int articleId,
        int warehouseId,
        decimal plannedQuantity,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException("Auftrag nicht gefunden.");

        var material = new OrderMaterial
        {
            OrderId = orderId,
            ArticleId = articleId,
            WarehouseId = warehouseId,
            PlannedQuantity = plannedQuantity,
            ActualQuantity = 0,
            IsReserved = false,
            IsConfirmed = false
        };

        return await orderRepository.AddOrderMaterialAsync(material, cancellationToken);
    }

    public async Task<OrderMaterial> ConfirmMaterialUsageAsync(
        int orderMaterialId,
        decimal actualQuantity,
        CancellationToken cancellationToken = default)
    {
        // OrderMaterial holen und aktualisieren
        var order = await orderRepository.GetByIdAsync(0, cancellationToken);
        // TODO: Implementierung wenn GetOrderMaterialById im Repository verfügbar
        throw new NotImplementedException("Materialbestätigung wird später implementiert");
    }

    public async Task<IEnumerable<Order>> SearchAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await orderRepository.SearchAsync(searchTerm, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        return await orderRepository.GetByStatusAsync(status, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByCustomerNumberAsync(
        string customerNumber,
        CancellationToken cancellationToken = default)
    {
        return await orderRepository.GetByCustomerNumberAsync(customerNumber, cancellationToken);
    }

    public Task<IEnumerable<WorkTimeEntry>> GetWorkTimeEntriesAsync(
        int orderId,
        CancellationToken cancellationToken = default)
        => orderRepository.GetWorkTimeEntriesAsync(orderId, cancellationToken);

    public Task DeleteWorkTimeEntryAsync(int entryId, CancellationToken cancellationToken = default)
        => orderRepository.DeleteWorkTimeEntryAsync(entryId, cancellationToken);

    public Task<IEnumerable<OrderMaterial>> GetOrderMaterialsAsync(
        int orderId,
        CancellationToken cancellationToken = default)
        => orderRepository.GetOrderMaterialsAsync(orderId, cancellationToken);

    // Hilfsmethoden
    private void CalculateTotals(Order order)
    {
        order.TotalNet = order.Products.Sum(p => p.TotalNet);
        order.TotalTaxAmount = order.Products.Sum(p => p.TaxAmount);
        order.TotalGross = order.Products.Sum(p => p.TotalGross);
    }

    private void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Erlaubte Status-Übergänge
        var allowedTransitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            { OrderStatus.Draft, new[] { OrderStatus.Planned, OrderStatus.Cancelled } },
            { OrderStatus.Planned, new[] { OrderStatus.InProgress, OrderStatus.Cancelled } },
            { OrderStatus.InProgress, new[] { OrderStatus.Completed, OrderStatus.Cancelled } },
            { OrderStatus.Completed, new[] { OrderStatus.Invoiced } },
            { OrderStatus.Invoiced, Array.Empty<OrderStatus>() },
            { OrderStatus.Cancelled, Array.Empty<OrderStatus>() }
        };

        if (!allowedTransitions[currentStatus].Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Status-Wechsel von {currentStatus} zu {newStatus} ist nicht erlaubt.");
        }
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        var count = await orderRepository.CountAsync(cancellationToken);
        var year = DateTime.UtcNow.Year;
        return $"AUT-{year}-{(count + 1).ToString().PadLeft(4, '0')}";
    }

    private async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Wenn InvoiceRepository Count implementiert hat
        var year = DateTime.UtcNow.Year;
        return $"RE-{year}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
    }
}
