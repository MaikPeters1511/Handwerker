using Handwerker.Application.Services;
using Handwerker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ArticlesController(
    IArticleService articleService,
    IInventoryService inventoryService,
    NotificationService notificationService) : ApiControllerBase
{    /// <summary>
    /// Lädt alle Artikel
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetArticles(CancellationToken cancellationToken = default)
    {
        var articles = await articleService.GetAllAsync(cancellationToken);
        return Ok(articles.Select(MapToDto));
    }

    /// <summary>
    /// Lädt alle aktiven Artikel
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveArticles(CancellationToken cancellationToken = default)
    {
        var articles = await articleService.GetActiveAsync(cancellationToken);
        return Ok(articles.Select(MapToDto));
    }

    /// <summary>
    /// Lädt Artikel mit niedrigem Bestand
    /// </summary>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockArticles(CancellationToken cancellationToken = default)
    {
        var articles = await inventoryService.GetLowStockArticlesAsync(cancellationToken);
        return Ok(articles.Select(MapToDto));
    }

    /// <summary>
    /// Sucht nach Artikeln
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchArticles([FromQuery] string term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest(new { error = "Suchbegriff darf nicht leer sein." });

        var articles = await articleService.SearchAsync(term, cancellationToken);
        return Ok(articles.Select(MapToDto));
    }

    /// <summary>
    /// Lädt einen spezifischen Artikel
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetArticle(int id, CancellationToken cancellationToken = default)
    {
        var article = await articleService.GetByIdAsync(id, cancellationToken);
        if (article is null)
            return NotFound(new { error = "Artikel nicht gefunden." });

        return Ok(MapToDetailDto(article));
    }

    /// <summary>
    /// Erstellt einen neuen Artikel
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequest request, CancellationToken cancellationToken = default)
    {
        if (await articleService.ExistsAsync(request.ArticleNumber, cancellationToken))
            return BadRequest(new { error = "Artikelnummer existiert bereits." });

        var article = new Article
        {
            ArticleNumber = request.ArticleNumber,
            Name = request.Name,
            Description = request.Description,
            Unit = request.Unit,
            UnitPrice = request.UnitPrice,
            TaxRate = request.TaxRate,
            Category = request.Category
        };

        var created = await articleService.CreateAsync(article, GetUserId(), cancellationToken);
        await notificationService.NotifyArticleCreatedAsync(GetUserId(), created.Id, created.Name);

        return CreatedAtAction(nameof(GetArticle), new { id = created.Id }, MapToDetailDto(created));
    }

    /// <summary>
    /// Aktualisiert einen Artikel
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateArticle(int id, [FromBody] UpdateArticleRequest request, CancellationToken cancellationToken = default)
    {
        if (id != request.Id)
            return BadRequest(new { error = "ID in URL und Body stimmen nicht überein." });

        var existing = await articleService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new { error = "Artikel nicht gefunden." });

        if (existing.ArticleNumber != request.ArticleNumber &&
            await articleService.ExistsAsync(request.ArticleNumber, cancellationToken))
            return BadRequest(new { error = "Artikelnummer existiert bereits." });

        existing.ArticleNumber = request.ArticleNumber;
        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Unit = request.Unit;
        existing.UnitPrice = request.UnitPrice;
        existing.TaxRate = request.TaxRate;
        existing.Category = request.Category;
        existing.IsActive = request.IsActive;

        await articleService.UpdateAsync(existing, cancellationToken);
        await notificationService.NotifyArticleUpdatedAsync(GetUserId(), existing.Id, existing.Name);

        return NoContent();
    }

    /// <summary>
    /// Löscht einen Artikel (Soft Delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteArticle(int id, CancellationToken cancellationToken = default)
    {
        var existing = await articleService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new { error = "Artikel nicht gefunden." });

        await articleService.DeleteAsync(id, cancellationToken);
        await notificationService.NotifyArticleDeletedAsync(GetUserId(), existing.Name);

        return NoContent();
    }

    #region Helper Methods

    private static ArticleDto MapToDto(Article article) => new()
    {
        Id = article.Id,
        ArticleNumber = article.ArticleNumber,
        Name = article.Name,
        Description = article.Description,
        Unit = article.Unit,
        UnitPrice = article.UnitPrice,
        TaxRate = article.TaxRate,
        Category = article.Category,
        IsActive = article.IsActive,
        TotalStock = article.TotalStock
    };

    private static ArticleDetailDto MapToDetailDto(Article article) => new()
    {
        Id = article.Id,
        ArticleNumber = article.ArticleNumber,
        Name = article.Name,
        Description = article.Description,
        Unit = article.Unit,
        UnitPrice = article.UnitPrice,
        TaxRate = article.TaxRate,
        Category = article.Category,
        IsActive = article.IsActive,
        TotalStock = article.TotalStock,
        WarehouseStocks = article.ArticleWarehouses?.Select(aw => new ArticleWarehouseDto
        {
            WarehouseId = aw.WarehouseId,
            WarehouseName = aw.Warehouse?.Name ?? "",
            StockQuantity = aw.StockQuantity,
            MinStockLevel = aw.MinStockLevel,
            MaxStockLevel = aw.MaxStockLevel,
            StorageLocation = aw.StorageLocation,
            IsLowStock = aw.IsLowStock
        }).ToList() ?? []
    };

    #endregion
}
