using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

public class Notification
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string UserId { get; set; } = string.Empty; // Keycloak User ID
    
    [Required]
    public NotificationType Type { get; set; } = NotificationType.Info;
    
    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty; // z.B. "Recipient", "Provider"
    
    public int? EntityId { get; set; } // ID der betroffenen Entität
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
