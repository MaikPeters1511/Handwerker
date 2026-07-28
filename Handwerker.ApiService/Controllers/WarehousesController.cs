using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehousesController(IWarehouseRepository warehouseRepository) : ControllerBase
{
    /// <summary>
    /// Lädt alle Lager
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWarehouses(CancellationToken cancellationToken = default)
    {
        try
        {
            var warehouses = await warehouseRepository.GetAllAsync(cancellationToken);
            var dtos = warehouses.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lädt alle aktiven Lager
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveWarehouses(CancellationToken cancellationToken = default)
    {
        try
        {
            var warehouses = await warehouseRepository.GetActiveAsync(cancellationToken);
            var dtos = warehouses.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lädt ein spezifisches Lager
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWarehouse(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var warehouse = await warehouseRepository.GetByIdAsync(id, cancellationToken);
            if (warehouse == null)
                return NotFound(new { error = "Lager nicht gefunden." });

            return Ok(MapToDetailDto(warehouse));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Erstellt ein neues Lager
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var warehouse = new Warehouse
            {
                Name = request.Name,
                Description = request.Description,
                Address = request.Address,
                IsActive = true
            };

            var created = await warehouseRepository.AddAsync(warehouse, cancellationToken);
            return CreatedAtAction(nameof(GetWarehouse), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Aktualisiert ein Lager
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] UpdateWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        if (id != request.Id)
            return BadRequest(new { error = "ID in URL und Body stimmen nicht überein." });

        try
        {
            var existing = await warehouseRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { error = "Lager nicht gefunden." });

            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.Address = request.Address;
            existing.IsActive = request.IsActive;

            await warehouseRepository.UpdateAsync(existing, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Löscht ein Lager (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWarehouse(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await warehouseRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { error = "Lager nicht gefunden." });

            await warehouseRepository.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lägt den Bestand eines Artikels in einem Lager
    /// </summary>
    [HttpGet("{warehouseId}/articles/{articleId}/stock")]
    public async Task<IActionResult> GetStock(int warehouseId, int articleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stock = await warehouseRepository.GetStockAsync(articleId, warehouseId, cancellationToken);
            if (stock == null)
                return Ok(new { stockQuantity = 0, minStockLevel = 0 });

            return Ok(new
            {
                stock.StockQuantity,
                stock.MinStockLevel,
                stock.MaxStockLevel,
                stock.StorageLocation,
                stock.IsLowStock
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #region Helper Methods

    private static WarehouseDto MapToDto(Warehouse warehouse) => new()
    {
        Id = warehouse.Id,
        Name = warehouse.Name,
        Description = warehouse.Description,
        Address = warehouse.Address,
        IsActive = warehouse.IsActive,
        CreatedAt = warehouse.CreatedAt
    };

    private static WarehouseDetailDto MapToDetailDto(Warehouse warehouse) => new()
    {
        Id = warehouse.Id,
        Name = warehouse.Name,
        Description = warehouse.Description,
        Address = warehouse.Address,
        IsActive = warehouse.IsActive,
        CreatedAt = warehouse.CreatedAt,
        ArticleCount = warehouse.ArticleWarehouses?.Count ?? 0
    };

    #endregion
}

#region DTOs

public record WarehouseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record WarehouseDetailDto : WarehouseDto
{
    public int ArticleCount { get; set; }
}

public record CreateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
}

public record UpdateWarehouseRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}

#endregion
