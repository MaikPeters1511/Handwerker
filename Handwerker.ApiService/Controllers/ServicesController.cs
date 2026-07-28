using Handwerker.Application.Services;
using Handwerker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[Route("api/services")]
[Authorize]
public class ServicesController(
    IServiceItemService serviceItemService,
    NotificationService notificationService,
    ILogger<ServicesController> logger) : ApiControllerBase
{
    /// <summary>
    /// Versendet die Benachrichtigung, ohne dass ein Fehler dabei den Erfolg der
    /// bereits abgeschlossenen Schreiboperation als 500 an den Client durchreicht.
    /// </summary>
    private async Task NotifySafelyAsync(Func<Task> notify, string action, int? entityId = null)
    {
        try
        {
            await notify();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Benachrichtigung für Leistung ({Action}, Id={EntityId}) konnte nicht versendet werden.", action, entityId);
        }
    }

    /// <summary>
    /// Lädt alle Leistungen
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetServices(CancellationToken cancellationToken = default)
    {
        var services = await serviceItemService.GetAllAsync(cancellationToken);
        return Ok(services.Select(MapToDto));
    }

    /// <summary>
    /// Lädt alle aktiven Leistungen
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveServices(CancellationToken cancellationToken = default)
    {
        var services = await serviceItemService.GetActiveAsync(cancellationToken);
        return Ok(services.Select(MapToDto));
    }

    /// <summary>
    /// Sucht nach Leistungen
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchServices([FromQuery] string term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest(new { error = "Suchbegriff darf nicht leer sein." });

        var services = await serviceItemService.SearchAsync(term, cancellationToken);
        return Ok(services.Select(MapToDto));
    }

    /// <summary>
    /// Lädt eine spezifische Leistung
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetService(int id, CancellationToken cancellationToken = default)
    {
        var service = await serviceItemService.GetByIdAsync(id, cancellationToken);
        if (service is null)
            return NotFound(new { error = "Leistung nicht gefunden." });

        return Ok(MapToDto(service));
    }

    /// <summary>
    /// Erstellt eine neue Leistung. Die Leistungsnummer wird automatisch vergeben.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceItemRequest request, CancellationToken cancellationToken = default)
    {
        var serviceItem = new ServiceItem
        {
            Name = request.Name,
            Description = request.Description,
            Unit = request.Unit,
            UnitPrice = request.UnitPrice,
            TaxRate = request.TaxRate
        };

        var created = await serviceItemService.CreateAsync(serviceItem, cancellationToken);
        await NotifySafelyAsync(
            () => notificationService.NotifyServiceItemCreatedAsync(GetUserId(), created.Id, created.Name),
            nameof(CreateService), created.Id);

        return CreatedAtAction(nameof(GetService), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>
    /// Aktualisiert eine Leistung
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceItemRequest request, CancellationToken cancellationToken = default)
    {
        if (id != request.Id)
            return BadRequest(new { error = "ID in URL und Body stimmen nicht überein." });

        var existing = await serviceItemService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new { error = "Leistung nicht gefunden." });

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Unit = request.Unit;
        existing.UnitPrice = request.UnitPrice;
        existing.TaxRate = request.TaxRate;
        existing.IsActive = request.IsActive;

        await serviceItemService.UpdateAsync(existing, cancellationToken);
        await NotifySafelyAsync(
            () => notificationService.NotifyServiceItemUpdatedAsync(GetUserId(), existing.Id, existing.Name),
            nameof(UpdateService), existing.Id);

        return NoContent();
    }

    /// <summary>
    /// Löscht eine Leistung (Soft Delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteService(int id, CancellationToken cancellationToken = default)
    {
        var existing = await serviceItemService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new { error = "Leistung nicht gefunden." });

        await serviceItemService.DeleteAsync(id, cancellationToken);
        await NotifySafelyAsync(
            () => notificationService.NotifyServiceItemDeletedAsync(GetUserId(), existing.Name),
            nameof(DeleteService), existing.Id);

        return NoContent();
    }

    private static ServiceItemDto MapToDto(ServiceItem serviceItem) => new()
    {
        Id = serviceItem.Id,
        ServiceNumber = serviceItem.ServiceNumber,
        Name = serviceItem.Name,
        Description = serviceItem.Description,
        Unit = serviceItem.Unit,
        UnitPrice = serviceItem.UnitPrice,
        TaxRate = serviceItem.TaxRate,
        IsActive = serviceItem.IsActive
    };
}
