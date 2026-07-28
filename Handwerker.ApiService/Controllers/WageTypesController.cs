using Handwerker.Application.Services;
using Handwerker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[Route("api/wagetypes")]
[Authorize]
public class WageTypesController(
    IWageTypeService wageTypeService,
    NotificationService notificationService,
    ILogger<WageTypesController> logger) : ApiControllerBase
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
            logger.LogWarning(ex, "Benachrichtigung für Lohnart ({Action}, Id={EntityId}) konnte nicht versendet werden.", action, entityId);
        }
    }

    /// <summary>
    /// Lädt alle Lohnarten
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWageTypes(CancellationToken cancellationToken = default)
    {
        var wageTypes = await wageTypeService.GetAllAsync(cancellationToken);
        return Ok(wageTypes.Select(MapToDto));
    }

    /// <summary>
    /// Lädt alle aktiven Lohnarten
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveWageTypes(CancellationToken cancellationToken = default)
    {
        var wageTypes = await wageTypeService.GetActiveAsync(cancellationToken);
        return Ok(wageTypes.Select(MapToDto));
    }

    /// <summary>
    /// Sucht nach Lohnarten
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchWageTypes([FromQuery] string term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest(new { error = "Suchbegriff darf nicht leer sein." });

        var wageTypes = await wageTypeService.SearchAsync(term, cancellationToken);
        return Ok(wageTypes.Select(MapToDto));
    }

    /// <summary>
    /// Lädt eine spezifische Lohnart
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWageType(int id, CancellationToken cancellationToken = default)
    {
        var wageType = await wageTypeService.GetByIdAsync(id, cancellationToken);
        if (wageType is null)
            return NotFound(new { error = "Lohnart nicht gefunden." });

        return Ok(MapToDto(wageType));
    }

    /// <summary>
    /// Erstellt eine neue Lohnart. Die Lohnartennummer wird automatisch vergeben.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWageType([FromBody] CreateWageTypeRequest request, CancellationToken cancellationToken = default)
    {
        var wageType = new WageType
        {
            Name = request.Name,
            Description = request.Description,
            HourlyRate = request.HourlyRate,
            TaxRate = request.TaxRate
        };

        var created = await wageTypeService.CreateAsync(wageType, cancellationToken);
        await NotifySafelyAsync(
            () => notificationService.NotifyWageTypeCreatedAsync(GetUserId(), created.Id, created.Name),
            nameof(CreateWageType), created.Id);

        return CreatedAtAction(nameof(GetWageType), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>
    /// Aktualisiert eine Lohnart
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateWageType(int id, [FromBody] UpdateWageTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (id != request.Id)
            return BadRequest(new { error = "ID in URL und Body stimmen nicht überein." });

        var existing = await wageTypeService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new { error = "Lohnart nicht gefunden." });

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.HourlyRate = request.HourlyRate;
        existing.TaxRate = request.TaxRate;
        existing.IsActive = request.IsActive;

        await wageTypeService.UpdateAsync(existing, cancellationToken);
        await NotifySafelyAsync(
            () => notificationService.NotifyWageTypeUpdatedAsync(GetUserId(), existing.Id, existing.Name),
            nameof(UpdateWageType), existing.Id);

        return NoContent();
    }

    /// <summary>
    /// Löscht eine Lohnart (Soft Delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteWageType(int id, CancellationToken cancellationToken = default)
    {
        var existing = await wageTypeService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new { error = "Lohnart nicht gefunden." });

        await wageTypeService.DeleteAsync(id, cancellationToken);
        await NotifySafelyAsync(
            () => notificationService.NotifyWageTypeDeletedAsync(GetUserId(), existing.Name),
            nameof(DeleteWageType), existing.Id);

        return NoContent();
    }

    private static WageTypeDto MapToDto(WageType wageType) => new()
    {
        Id = wageType.Id,
        WageNumber = wageType.WageNumber,
        Name = wageType.Name,
        Description = wageType.Description,
        HourlyRate = wageType.HourlyRate,
        TaxRate = wageType.TaxRate,
        IsActive = wageType.IsActive
    };
}
