using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class SettingsRepository(HandwerkerDbContext db) : ISettingsRepository
{
    public async Task<UserSettings> GetSettingsAsync(string userId, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[SettingsRepository] GetSettingsAsync: Suche Settings für UserId: {userId}");
        var result = await db.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId, cancellationToken);
        
        if (result != null)
        {
            Console.WriteLine($"[SettingsRepository] GetSettingsAsync: Settings gefunden - ID: {result.Id}, Theme: {result.Theme}");
        }
        else
        {
            Console.WriteLine($"[SettingsRepository] GetSettingsAsync: Keine Settings gefunden für UserId: {userId}");
        }
        
        return result;
    }
    
    public async Task CreateSettingsAsync(UserSettings defaultSettings, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[SettingsRepository] CreateSettingsAsync: Erstelle Settings für UserId: {defaultSettings.UserId}");
        db.UserSettings.Add(defaultSettings);
        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[SettingsRepository] CreateSettingsAsync: Settings erstellt - ID: {defaultSettings.Id}");
    }

    public async Task UpdateSettingsAsync(UserSettings request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[SettingsRepository] UpdateSettingsAsync: Aktualisiere Settings für UserId: {request.UserId}");
        
        var settings = await db.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == request.UserId, cancellationToken);
            
        if (settings == null)
        {
            Console.WriteLine($"[SettingsRepository] UpdateSettingsAsync: FEHLER - Settings nicht gefunden für UserId: {request.UserId}");
            throw new InvalidOperationException("Settings not found");
        }
        
        Console.WriteLine($"[SettingsRepository] UpdateSettingsAsync: Settings gefunden (ID: {settings.Id}), aktualisiere Werte...");
        
        settings.Theme = request.Theme;
        settings.LanguageCode = request.LanguageCode;
        settings.EmailNotifications = request.EmailNotifications;
        settings.PushNotifications = request.PushNotifications;
        settings.SmsNotifications = request.SmsNotifications;
        settings.TestEmail = request.TestEmail;
        settings.TestEmailSubject = request.TestEmailSubject;
        settings.TestEmailBody = request.TestEmailBody;
        settings.InvoicePrefix = request.InvoicePrefix;
        settings.NextInvoiceNumber = request.NextInvoiceNumber;
        settings.TaxRate = request.TaxRate;
        settings.Currency = request.Currency;
        settings.UpdatedAt = DateTime.UtcNow;
        
        Console.WriteLine($"[SettingsRepository] UpdateSettingsAsync: Werte aktualisiert, speichere in DB...");
        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[SettingsRepository] UpdateSettingsAsync: Erfolgreich gespeichert für UserId: {request.UserId}");
    }
}