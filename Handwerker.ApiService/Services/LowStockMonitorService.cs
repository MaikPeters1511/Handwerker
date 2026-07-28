using Handwerker.Application.Services;
using Handwerker.Domain.Interfaces;

namespace Handwerker.ApiService.Services;

/// <summary>
/// Background Service für die Überwachung von niedrigem Lagerbestand
/// </summary>
public class LowStockMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LowStockMonitorService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Alle 5 Minuten prüfen

    public LowStockMonitorService(
        IServiceProvider serviceProvider,
        ILogger<LowStockMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LowStockMonitorService gestartet");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckLowStockItems(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei der Low-Stock-Überwachung");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckLowStockItems(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        var lowStockArticles = await inventoryService.GetLowStockArticlesAsync(cancellationToken);

        foreach (var article in lowStockArticles)
        {
            foreach (var warehouseStock in article.ArticleWarehouses.Where(aw => aw.IsLowStock))
            {
                _logger.LogWarning(
                    "Low Stock Alert: Artikel '{ArticleName}' in Lager '{WarehouseName}' - Bestand: {Stock}, Mindestbestand: {MinStock}",
                    article.Name,
                    warehouseStock.Warehouse?.Name ?? "Unbekannt",
                    warehouseStock.StockQuantity,
                    warehouseStock.MinStockLevel);

                // Hier könnten wir auch SignalR-Benachrichtigungen senden
                // Dafür müssten wir den NotificationHub injecten
            }
        }
    }
}
