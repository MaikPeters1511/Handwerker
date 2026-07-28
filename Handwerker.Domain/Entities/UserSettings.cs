using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

public record UserSettings
{
    public int Id { get; set; }

    /// <summary>
    /// Keycloak User ID (Sub Claim)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Theme { get; set; } = "light";

    [MaxLength(10)]
    public string LanguageCode { get; set; } = "de";

    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = false;
    public bool SmsNotifications { get; set; } = false;

    /// <summary>
    /// Test-E-Mail-Adresse für Mailpit (optional)
    /// </summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? TestEmail { get; set; }

    /// <summary>
    /// Betreff für Test-E-Mail
    /// </summary>
    [MaxLength(500)]
    public string? TestEmailSubject { get; set; }

    /// <summary>
    /// Text/Inhalt für Test-E-Mail
    /// </summary>
    public string? TestEmailBody { get; set; }

    [MaxLength(20)]
    public string InvoicePrefix { get; set; } = "RE-";

    public int NextInvoiceNumber { get; set; } = 1001;

    public decimal TaxRate { get; set; } = 19m;

    [MaxLength(10)]
    public string Currency { get; set; } = "EUR";

    // Installation Wizard fields
    [MaxLength(50)]
    public string Salutation { get; set; } = string.Empty; // Anrede
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty; // Titel
    [MaxLength(255)]
    public string FirstName { get; set; } = string.Empty; // Vorname
    [MaxLength(255)]
    public string LastName { get; set; } = string.Empty; // Nachname
    public string? ProfileImagePath { get; set; } // Profilbild path

    [MaxLength(255)]
    public string Industry { get; set; } = string.Empty; // Branche
    [MaxLength(500)]
    public string ReferralSource { get; set; } = string.Empty; // Wie aufmerksam geworden
    public bool AvAgreementAccepted { get; set; } // AV-Vertrag Zustimmung
    public bool IsInstallationCompleted { get; set; } // Installation abgeschlossen

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
