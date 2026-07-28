namespace Handwerker.Domain.Entities;

/// <summary>
/// Priorität für Aufträge
/// </summary>
public enum Priority
{
    /// <summary>
    /// Niedrig
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Hoch
    /// </summary>
    High = 2,

    /// <summary>
    /// Eilig
    /// </summary>
    Urgent = 3
}
