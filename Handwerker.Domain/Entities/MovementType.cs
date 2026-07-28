namespace Handwerker.Domain.Entities;

/// <summary>
/// Typ einer Lagerbewegung
/// </summary>
public enum MovementType
{
    /// <summary>
    /// Wareneingang
    /// </summary>
    In = 0,

    /// <summary>
    /// Warenausgang (Entnahme)
    /// </summary>
    Out = 1,

    /// <summary>
    /// Bestandskorrektur
    /// </summary>
    Adjustment = 2,

    /// <summary>
    /// Reservierung (noch nicht abgebucht)
    /// </summary>
    Reservation = 3,

    /// <summary>
    /// Reservierung aufgehoben
    /// </summary>
    ReservationCancelled = 4,

    /// <summary>
    /// Reservierung bestätigt (wird zu Out)
    /// </summary>
    ReservationConfirmed = 5
}
