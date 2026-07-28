using Handwerker.Application.Services;
using Handwerker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[Route("api/[controller]")]
[Authorize]
public class OrdersController(
    IOrderService orderService,
    NotificationService notificationService) : ApiControllerBase
{    /// <summary>
    /// Lädt alle Aufträge
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await orderService.GetAllAsync(cancellationToken);
            var dtos = orders.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lädt Aufträge nach Status
    /// </summary>
    [HttpGet("by-status/{status}")]
    public async Task<IActionResult> GetOrdersByStatus(OrderStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await orderService.GetByStatusAsync(status, cancellationToken);
            var dtos = orders.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Sucht nach Aufträgen
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchOrders([FromQuery] string term, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { error = "Suchbegriff darf nicht leer sein." });

            var orders = await orderService.SearchAsync(term, cancellationToken);
            var dtos = orders.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lädt einen spezifischen Auftrag
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await orderService.GetByIdAsync(id, cancellationToken);
            if (order == null)
                return NotFound(new { error = "Auftrag nicht gefunden." });

            return Ok(MapToDetailDto(order));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Erstellt einen neuen Auftrag
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "system";
            
            var order = new Order
            {
                OrderDate = request.OrderDate,
                CustomerNumber = request.CustomerNumber,
                Recipient = request.Recipient,
                Provider = request.Provider,
                Products = request.Products,
                Priority = request.Priority,
                PlannedStartDate = request.PlannedStartDate,
                PlannedEndDate = request.PlannedEndDate,
                EstimatedHours = request.EstimatedHours,
                Description = request.Description,
                InternalNotes = request.InternalNotes
            };

            var created = await orderService.CreateAsync(order, userId, cancellationToken);
            
            await notificationService.NotifyOrderCreatedAsync(userId, created.Id, created.OrderNumber);

            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, MapToDetailDto(created));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Erstellt einen Auftrag aus mehreren Angeboten
    /// </summary>
    [HttpPost("from-offers")]
    public async Task<IActionResult> CreateFromOffers([FromBody] CreateOrderFromOffersRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "system";

            var orderData = new Order
            {
                OrderDate = request.OrderDate,
                Priority = request.Priority,
                PlannedStartDate = request.PlannedStartDate,
                PlannedEndDate = request.PlannedEndDate,
                EstimatedHours = request.EstimatedHours,
                Description = request.Description,
                InternalNotes = request.InternalNotes
            };

            var created = await orderService.CreateFromOffersAsync(
                request.OfferIds, 
                orderData, 
                userId, 
                cancellationToken);

            await notificationService.NotifyOrderCreatedAsync(userId, created.Id, created.OrderNumber);

            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, MapToDetailDto(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Aktualisiert einen Auftrag
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (id != request.Id)
            return BadRequest(new { error = "ID in URL und Body stimmen nicht überein." });

        try
        {
            var existing = await orderService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { error = "Auftrag nicht gefunden." });

            existing.OrderDate = request.OrderDate;
            existing.CustomerNumber = request.CustomerNumber;
            existing.Recipient = request.Recipient;
            existing.Provider = request.Provider;
            existing.Products = request.Products;
            existing.Priority = request.Priority;
            existing.PlannedStartDate = request.PlannedStartDate;
            existing.PlannedEndDate = request.PlannedEndDate;
            existing.EstimatedHours = request.EstimatedHours;
            existing.Description = request.Description;
            existing.InternalNotes = request.InternalNotes;

            var updated = await orderService.UpdateAsync(existing, cancellationToken);
            
            var userId = User.FindFirst("sub")?.Value ?? "system";
            await notificationService.NotifyOrderUpdatedAsync(userId, updated.Id, updated.OrderNumber);

            return Ok(MapToDetailDto(updated));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Löscht einen Auftrag
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await orderService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { error = "Auftrag nicht gefunden." });

            await orderService.DeleteAsync(id, cancellationToken);
            
            var userId = User.FindFirst("sub")?.Value ?? "system";
            await notificationService.NotifyOrderDeletedAsync(userId, existing.OrderNumber);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Ändert den Status eines Auftrags
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "system";
            var updated = await orderService.UpdateStatusAsync(id, request.Status, userId, cancellationToken);
            
            return Ok(MapToDetailDto(updated));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Wandelt einen Auftrag in eine Rechnung um
    /// </summary>
    [HttpPost("{id}/convert-to-invoice")]
    public async Task<IActionResult> ConvertToInvoice(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "system";
            var invoice = await orderService.ConvertToInvoiceAsync(id, userId, cancellationToken);
            
            await notificationService.NotifyInvoiceCreatedAsync(userId, invoice.Id, invoice.InvoiceNumber);

            return Ok(new { invoiceId = invoice.Id, invoiceNumber = invoice.InvoiceNumber });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #region Work Time

    /// <summary>
    /// Fügt einen Arbeitszeiteintrag hinzu
    /// </summary>
    [HttpPost("{orderId:int}/worktime")]
    public async Task<IActionResult> AddWorkTimeEntry(int orderId, [FromBody] WorkTimeEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = new WorkTimeEntry
        {
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            BreakDuration = request.BreakDuration,
            Description = request.Description,
            IsBillable = request.IsBillable,
            HourlyRate = request.HourlyRate,
            UserId = GetUserId(),
            UserName = GetUserName()
        };

        var created = await orderService.AddWorkTimeEntryAsync(orderId, entry, cancellationToken);
        return Ok(created);
    }

    /// <summary>
    /// Lädt alle Arbeitszeiteinträge eines Auftrags
    /// </summary>
    [HttpGet("{orderId:int}/worktime")]
    public async Task<IActionResult> GetWorkTimeEntries(int orderId, CancellationToken cancellationToken = default)
    {
        var entries = await orderService.GetWorkTimeEntriesAsync(orderId, cancellationToken);
        return Ok(entries);
    }

    /// <summary>
    /// Löscht einen Arbeitszeiteintrag
    /// </summary>
    [HttpDelete("worktime/{entryId:int}")]
    public async Task<IActionResult> DeleteWorkTimeEntry(int entryId, CancellationToken cancellationToken = default)
    {
        await orderService.DeleteWorkTimeEntryAsync(entryId, cancellationToken);
        return NoContent();
    }

    #endregion

    #region Materials

    /// <summary>
    /// Fügt Material zu einem Auftrag hinzu
    /// </summary>
    [HttpPost("{orderId:int}/materials")]
    public async Task<IActionResult> AddMaterial(int orderId, [FromBody] AddMaterialRequest request, CancellationToken cancellationToken = default)
    {
        var material = await orderService.AddMaterialAsync(
            orderId,
            request.ArticleId,
            request.WarehouseId,
            request.PlannedQuantity,
            cancellationToken);

        return Ok(material);
    }

    /// <summary>
    /// Lädt alle Materialien eines Auftrags
    /// </summary>
    [HttpGet("{orderId:int}/materials")]
    public async Task<IActionResult> GetMaterials(int orderId, CancellationToken cancellationToken = default)
    {
        var materials = await orderService.GetOrderMaterialsAsync(orderId, cancellationToken);
        return Ok(materials);
    }

    /// <summary>
    /// Bestätigt die Material-Entnahme
    /// </summary>
    [HttpPost("materials/{materialId:int}/confirm")]
    public async Task<IActionResult> ConfirmMaterial(int materialId, [FromBody] ConfirmMaterialRequest request, CancellationToken cancellationToken = default)
    {
        var material = await orderService.ConfirmMaterialUsageAsync(
            materialId,
            request.ActualQuantity,
            cancellationToken);

        return Ok(material);
    }

    #endregion

    #region Helper Methods

    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        OrderDate = order.OrderDate,
        CustomerNumber = order.CustomerNumber,
        RecipientName = order.Recipient?.Name ?? "",
        Status = order.Status,
        Priority = order.Priority,
        TotalNet = order.TotalNet,
        TotalGross = order.TotalGross,
        PlannedStartDate = order.PlannedStartDate,
        PlannedEndDate = order.PlannedEndDate,
        ActualStartDate = order.ActualStartDate,
        ActualEndDate = order.ActualEndDate,
        EstimatedHours = order.EstimatedHours,
        ActualHours = order.ActualHours,
        InvoiceId = order.InvoiceId,
        IsPaid = order.IsPaid
    };

    private static OrderDetailDto MapToDetailDto(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        OrderDate = order.OrderDate,
        CustomerNumber = order.CustomerNumber,
        Recipient = order.Recipient,
        Provider = order.Provider,
        Status = order.Status,
        Priority = order.Priority,
        TotalNet = order.TotalNet,
        TotalTaxAmount = order.TotalTaxAmount,
        TotalGross = order.TotalGross,
        PlannedStartDate = order.PlannedStartDate,
        PlannedEndDate = order.PlannedEndDate,
        ActualStartDate = order.ActualStartDate,
        ActualEndDate = order.ActualEndDate,
        EstimatedHours = order.EstimatedHours,
        ActualHours = order.ActualHours,
        Description = order.Description,
        InternalNotes = order.InternalNotes,
        Products = order.Products,
        SourceOffers = order.SourceOffers?.Select(oo => new OrderOfferDto
        {
            OfferId = oo.OfferId,
            OfferNumber = oo.Offer?.OfferNumber ?? "",
            PortionPercentage = oo.PortionPercentage
        }).ToList() ?? new List<OrderOfferDto>(),
        Materials = order.Materials,
        WorkTimeEntries = order.WorkTimeEntries,
        InvoiceId = order.InvoiceId,
        IsPaid = order.IsPaid,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt
    };

    #endregion
}
