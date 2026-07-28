using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Handwerker.Application.Services;

public interface IEmailService
{
    Task SendTestEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}

public class MailpitEmailService(IConfiguration configuration, ILogger<MailpitEmailService> logger)
    : IEmailService
{
    public async Task SendTestEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            // Mailpit SMTP-Konfiguration (Standard: localhost:1025)
            var smtpHost = configuration["Email:SmtpHost"] ?? "localhost";
            var smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "1025");
            var fromEmail = configuration["Email:FromEmail"] ?? "noreply@handwerker.local";
            var fromName = configuration["Email:FromName"] ?? "Handwerker App";

            using var smtpClient = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = false, // Mailpit benötigt kein SSL
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                // Mailpit hat keine Authentifizierung in der Standardkonfiguration
                Credentials = null
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            logger.LogInformation("Test-E-Mail erfolgreich an {Email} gesendet", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Senden der Test-E-Mail an {Email}", to);
            throw;
        }
    }
}
