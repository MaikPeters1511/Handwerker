namespace Handwerker.Domain.Entities;

public enum OfferStatus
{
    Draft = 0,      // Entwurf
    Sent = 1,       // Versendet
    Accepted = 2,   // Angenommen
    Declined = 3,   // Abgelehnt
    Converted = 4   // In Auftrag umgewandelt
}