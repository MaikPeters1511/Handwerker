using Microsoft.AspNetCore.SignalR;

namespace Handwerker.ApiService.Hubs;

/// <summary>
/// SignalR Hub für Echtzeit-Benachrichtigungen
/// </summary>
public class NotificationHub : Hub
{
    /// <summary>
    /// Benutzer tritt einer Gruppe bei (z.B. für rollenbasierte Benachrichtigungen)
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Benutzer verlässt eine Gruppe
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Sendet eine Benachrichtigung an alle verbundenen Clients
    /// </summary>
    public async Task BroadcastNotification(string title, string message, string type = "info")
    {
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            title,
            message,
            type,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sendet eine Low-Stock Warnung an alle Clients
    /// </summary>
    public async Task NotifyLowStock(int articleId, string articleName, string warehouseName, decimal currentStock, decimal minStockLevel)
    {
        await Clients.All.SendAsync("LowStockAlert", new
        {
            articleId,
            articleName,
            warehouseName,
            currentStock,
            minStockLevel,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sendet eine Order-Status-Änderung an betroffene Benutzer
    /// </summary>
    public async Task NotifyOrderStatusChanged(int orderId, string orderNumber, string newStatus, string message)
    {
        await Clients.Group($"order-{orderId}").SendAsync("OrderStatusChanged", new
        {
            orderId,
            orderNumber,
            newStatus,
            message,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sendet eine allgemeine Info an einen spezifischen Benutzer
    /// </summary>
    public async Task SendToUser(string userId, string title, string message, string type = "info")
    {
        await Clients.User(userId).SendAsync("PersonalNotification", new
        {
            title,
            message,
            type,
            timestamp = DateTime.UtcNow
        });
    }
}
