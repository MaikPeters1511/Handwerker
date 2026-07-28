namespace Handwerker.Domain.Entities;

/// <summary>
/// Auftragsstatus
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Entwurf
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Geplant
    /// </summary>
    Planned = 1,

    /// <summary>
    /// In Bearbeitung
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Abgeschlossen
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Abgerechnet
    /// </summary>
    Invoiced = 4,

    /// <summary>
    /// Storniert
    /// </summary>
    Cancelled = 5
}
