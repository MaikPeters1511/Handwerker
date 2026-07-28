using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

public class NotificationService(INotificationRepository  repo)
{
    public async Task<IEnumerable<Notification>> GetNotificationsAsync(string userId, int skip, int take, bool? isRead)
    {
        return await repo.GetNotificationsAsync(userId, skip, take, isRead);
    }

    public async Task<int> CountUnreadNotificationsAsync(string userId)
    {
        return await repo.CountUnreadNotificationsAsync(userId);
    }

    public async Task MarkAsReadAsync(int notiId, string userId)
    {
        await repo.MarkAsReadAsync(notiId, userId);
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await repo.MarkAllAsRead(userId);
    }

    public async Task DeleteNotificationAsync(int id, string userId)
    {
        await repo.DeleteNotificationAsync(id , userId);
    }

    public async Task DeleteAllNotificationAsync(string userId)
    {
        await repo.DeleteAllNotificationAsync(userId);
    }
    
    private async Task<Notification> CreateNotificationAsync(
        string userId,
        NotificationType type,
        string message,
        string entityType = "",
        int? entityId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            EntityType = entityType,
            EntityId = entityId,
            CreatedAt = DateTime.UtcNow
        };

        await repo.CreateNotificationAsync(notification);
        return notification;
    }

    // Helper-Methoden für häufige Szenarien
    public Task NotifyRecipientCreatedAsync(string userId, int recipientId, string recipientName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Kunde '{recipientName}' wurde erfolgreich angelegt.",
            "Recipient",
            recipientId);

    public Task NotifyRecipientUpdatedAsync(string userId, int recipientId, string recipientName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Kunde '{recipientName}' wurde erfolgreich aktualisiert.",
            "Recipient",
            recipientId);

    public Task NotifyRecipientDeletedAsync(string userId, string recipientName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Info,
            $"Kunde '{recipientName}' wurde gelöscht.",
            "Recipient");

    public Task NotifyProviderCreatedAsync(string userId, int providerId, string providerName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Lieferant '{providerName}' wurde erfolgreich angelegt.",
            "Provider",
            providerId);

    public Task NotifyProviderUpdatedAsync(string userId, int providerId, string providerName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Lieferant '{providerName}' wurde erfolgreich aktualisiert.",
            "Provider",
            providerId);

    public Task NotifyProviderDeletedAsync(string userId, string providerName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Info,
            $"Lieferant '{providerName}' wurde gelöscht.",
            "Provider");

    public Task NotifyProductCreatedAsync(string userId, int productId, string productName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Produkt '{productName}' wurde erfolgreich angelegt.",
            "Product",
            productId);

    public Task NotifyProductUpdatedAsync(string userId, int productId, string productName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Produkt '{productName}' wurde erfolgreich aktualisiert.",
            "Product",
            productId);

    public Task NotifyProductDeletedAsync(string userId, string productName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Info,
            $"Produkt '{productName}' wurde gelöscht.",
            "Product");

    public Task NotifyErrorAsync(string userId, string message)
        => CreateNotificationAsync(
            userId,
            NotificationType.Error,
            message);

    // Bank-Benachrichtigungen
    public Task NotifyBankCreatedAsync(string userId, int bankId, string bankName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Bank '{bankName}' wurde erfolgreich angelegt.",
            "Bank",
            bankId);

    public Task NotifyBankUpdatedAsync(string userId, int bankId, string bankName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Bank '{bankName}' wurde erfolgreich aktualisiert.",
            "Bank",
            bankId);

    public Task NotifyBankDeletedAsync(string userId, string bankName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Info,
            $"Bank '{bankName}' wurde gelöscht.",
            "Bank");

    // Invoice-Benachrichtigungen
    public Task NotifyInvoiceCreatedAsync(string userId, int invoiceId, string invoiceNumber)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Rechnung '{invoiceNumber}' wurde erfolgreich erstellt.",
            "Invoice",
            invoiceId);

    public Task NotifyInvoiceUpdatedAsync(string userId, int invoiceId, string invoiceNumber)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Rechnung '{invoiceNumber}' wurde erfolgreich aktualisiert.",
            "Invoice",
            invoiceId);

    public Task NotifyInvoiceDeletedAsync(string userId, string invoiceNumber)
        => CreateNotificationAsync(
            userId,
            NotificationType.Info,
            $"Rechnung '{invoiceNumber}' wurde gelöscht.",
            "Invoice");

    // Offer-Benachrichtigungen
    public Task NotifyOfferCreatedAsync(string userId, int offerId, string offerNumber)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Angebot '{offerNumber}' wurde erfolgreich erstellt.",
            "Offer",
            offerId);

    public Task NotifyOfferUpdatedAsync(string userId, int offerId, string offerNumber)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Angebot '{offerNumber}' wurde erfolgreich aktualisiert.",
            "Offer",
            offerId);

    public Task NotifyOfferDeletedAsync(string userId, string offerNumber)
        => CreateNotificationAsync(
            userId,
            NotificationType.Info,
            $"Angebot '{offerNumber}' wurde gelöscht.",
            "Offer");

    public Task NotifyOfferConvertedAsync(string userId, int offerId)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Angebot wurde erfolgreich in einen Auftrag umgewandelt.",
            "Offer",
            offerId);

    // Article-Benachrichtigungen
    public Task NotifyArticleCreatedAsync(string userId, int articleId, string articleName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Artikel '{articleName}' wurde erfolgreich erstellt.",
            "Article",
            articleId);

    public Task NotifyArticleUpdatedAsync(string userId, int articleId, string articleName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Artikel '{articleName}' wurde erfolgreich aktualisiert.",
            "Article",
            articleId);

    public Task NotifyArticleDeletedAsync(string userId, string articleName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Info,
            $"Artikel '{articleName}' wurde deaktiviert.",
            "Article");

    // Order-Benachrichtigungen
    public Task NotifyOrderCreatedAsync(string userId, int orderId, string orderNumber)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Auftrag '{orderNumber}' wurde erfolgreich erstellt.",
            "Order",
            orderId);

    public Task NotifyOrderUpdatedAsync(string userId, int orderId, string orderNumber)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Auftrag '{orderNumber}' wurde erfolgreich aktualisiert.",
            "Order",
            orderId);

    public Task NotifyOrderDeletedAsync(string userId, string orderNumber)
        => CreateNotificationAsync(
            userId,
            NotificationType.Info,
            $"Auftrag '{orderNumber}' wurde gelöscht.",
            "Order");

    // ServiceItem-Benachrichtigungen
    public Task NotifyServiceItemCreatedAsync(string userId, int serviceItemId, string serviceItemName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Leistung '{serviceItemName}' wurde erfolgreich erstellt.",
            "ServiceItem",
            serviceItemId);

    public Task NotifyServiceItemUpdatedAsync(string userId, int serviceItemId, string serviceItemName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Leistung '{serviceItemName}' wurde erfolgreich aktualisiert.",
            "ServiceItem",
            serviceItemId);

    public Task NotifyServiceItemDeletedAsync(string userId, string serviceItemName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Info,
            $"Leistung '{serviceItemName}' wurde deaktiviert.",
            "ServiceItem");

    // WageType-Benachrichtigungen
    public Task NotifyWageTypeCreatedAsync(string userId, int wageTypeId, string wageTypeName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Lohnart '{wageTypeName}' wurde erfolgreich erstellt.",
            "WageType",
            wageTypeId);

    public Task NotifyWageTypeUpdatedAsync(string userId, int wageTypeId, string wageTypeName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Success,
            $"Lohnart '{wageTypeName}' wurde erfolgreich aktualisiert.",
            "WageType",
            wageTypeId);

    public Task NotifyWageTypeDeletedAsync(string userId, string wageTypeName)
        => CreateNotificationAsync(
            userId,
            NotificationType.Info,
            $"Lohnart '{wageTypeName}' wurde deaktiviert.",
            "WageType");
}