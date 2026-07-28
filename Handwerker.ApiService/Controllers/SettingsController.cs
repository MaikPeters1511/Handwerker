using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Handwerker.ApiService.Models;
using Handwerker.Application.Services;
using Handwerker.Application.Services.Keycloak;
using Handwerker.Domain.Entities;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController(
    SettingsService settingsService,
    IKcUserService userService,
    IEmailService emailService) : ControllerBase
{
    /// <summary>
    /// Lädt die benutzerspezifischen Einstellungen für den aktuell eingeloggten User
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken = default)
    {
        var userId = userService.GetCurrentUserId();

        if (userId == null)
        {
            Console.WriteLine("[SettingsController] GetSettings: UserId ist null");
            return BadRequest();
        }

        Console.WriteLine($"[SettingsController] GetSettings: Lade Settings für UserId: {userId}");

        var result = await settingsService.GetSettingsAsync(userId, cancellationToken);

        if (result is not null)
        {
            Console.WriteLine($"[SettingsController] GetSettings: Settings gefunden - Theme: {result.Theme}, Language: {result.LanguageCode}");
            return Ok(result);
        }

        Console.WriteLine($"[SettingsController] GetSettings: Keine Settings gefunden, erstelle Default-Settings für UserId: {userId}");
        
        // Neue User bekommen Default-Settings
        var defaultSettings = CreateDefaultUserSettings(userId);
        try
        {
            await settingsService.CreateSettingsAsync(defaultSettings, cancellationToken);
            Console.WriteLine($"[SettingsController] GetSettings: Default-Settings erstellt für UserId: {userId}");
            return Ok(defaultSettings);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SettingsController] GetSettings: Fehler beim Erstellen der Default-Settings: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Speichert die benutzerspezifischen Einstellungen
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<UserSettings>> PutSettings(
        [FromBody] UserSettingsUpdateRequest? request, 
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return BadRequest("Request darf nicht null sein.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = userService.GetCurrentUserId();

        if (userId == null)
        {
            Console.WriteLine("[SettingsController] PutSettings: UserId ist null");
            return BadRequest();
        }

        Console.WriteLine($"[SettingsController] PutSettings: Speichere Settings für UserId: {userId}, Theme: {request.Theme}, Language: {request.LanguageCode}");

        var settings = await settingsService.GetSettingsAsync(userId, cancellationToken);

        if (settings is null)
        {
            Console.WriteLine($"[SettingsController] PutSettings: Keine Settings gefunden, erstelle neue für UserId: {userId}");
            settings = CreateDefaultUserSettings(userId);
            try
            {
                await settingsService.CreateSettingsAsync(settings, cancellationToken);
                Console.WriteLine($"[SettingsController] PutSettings: Settings erstellt für UserId: {userId}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SettingsController] PutSettings: Fehler beim Erstellen: {e.Message}");
            }
        }

        // Update Settings
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

        Console.WriteLine($"[SettingsController] PutSettings: Settings aktualisiert, rufe UpdateSettingsAsync auf...");

        try
        {
            await settingsService.UpdateSettingsAsync(settings, cancellationToken);
            Console.WriteLine($"[SettingsController] PutSettings: Settings erfolgreich gespeichert für UserId: {userId}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SettingsController] PutSettings: Fehler beim Speichern: {e.Message}");
            throw;
        }

        return Ok(settings);
    }

    /// <summary>
    /// Alternative POST-Methode für Kompatibilität
    /// </summary>
    [HttpPost("save")]
    public async Task<ActionResult<UserSettings>> SaveSettings(
        [FromBody] UserSettingsUpdateRequest? request, 
        CancellationToken cancellationToken = default)
    {
        // Ruft einfach PUT auf
        return await PutSettings(request, cancellationToken);
    }

    /// <summary>
    /// Erstellt Default-Settings für einen neuen User
    /// </summary>
    private static UserSettings CreateDefaultUserSettings(string userId)
    {
        return new UserSettings
        {
            UserId = userId,
            Theme = "light",
            LanguageCode = "de",
            EmailNotifications = true,
            PushNotifications = false,
            SmsNotifications = false,
            InvoicePrefix = "RE-",
            NextInvoiceNumber = 1001,
            TaxRate = 19m,
            Currency = "EUR",
            IsInstallationCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Sendet eine Test-E-Mail an die konfigurierte Adresse
    /// </summary>
    [HttpPost("send-test-email")]
    public async Task<IActionResult> SendTestEmail(
        [FromBody] SendTestEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return BadRequest("Request darf nicht null sein.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await emailService.SendTestEmailAsync(
                request.To,
                request.Subject,
                request.Body,
                cancellationToken
            );

            return Ok(new { message = "Test-E-Mail erfolgreich gesendet" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Fehler beim Senden der E-Mail", error = ex.Message });
        }
    }
}