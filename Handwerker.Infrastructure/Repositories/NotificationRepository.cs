using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class NotificationRepository(HandwerkerDbContext db) : INotificationRepository
{
    public async Task<IEnumerable<Notification>> GetNotificationsAsync(string userId, int skip, int take, bool? isRead)
    {
        var query = db.Notifications
            .Where(n => n.UserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountUnreadNotificationsAsync(string userId)
        => await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();

    public async Task MarkAsReadAsync(int notiId, string userId)
    {
        var notification = await GetByIdAsync(notiId);
        if (notification == null || notification.UserId != userId)
        {
            throw new InvalidOperationException();
        }
        
        notification.IsRead = true;
        await db.SaveChangesAsync();
    }

    public Task<Notification?> GetByIdAsync(int id)
    {
        var notification = db.Notifications.Find(id);
        return Task.FromResult(notification);
    }

    public async Task MarkAllAsRead(string userId)
    {
        await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));

    }

    public async Task DeleteNotificationAsync(int notiId, string userId)
    {
        var notification = await GetByIdAsync(notiId);
        if (notification == null || notification.UserId != userId)
        {
            throw new InvalidOperationException();
        }
        
        db.Notifications.Remove(notification);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAllNotificationAsync(string userId)
    {
        await db.Notifications
            .Where(n => n.UserId == userId)
            .ExecuteDeleteAsync();

    }

    public async Task CreateNotificationAsync(Notification notification)
    {
        await db.Notifications.AddAsync(notification);
        await db.SaveChangesAsync();
    }
}