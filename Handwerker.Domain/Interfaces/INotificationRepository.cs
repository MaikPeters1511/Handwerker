using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetNotificationsAsync(string userId, int skip, int take, bool? isRead);
    Task<int> CountUnreadNotificationsAsync(string userId);
    Task MarkAsReadAsync(int notiId, string userId);
    
    Task<Notification?> GetByIdAsync(int id);
    Task MarkAllAsRead(string userId);
    Task DeleteNotificationAsync(int id, string userId);
    Task DeleteAllNotificationAsync(string userId);
    Task CreateNotificationAsync(Notification notification);
}