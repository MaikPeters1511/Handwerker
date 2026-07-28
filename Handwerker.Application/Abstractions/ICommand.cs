namespace Handwerker.Application.Abstractions;

/// <summary>
/// Marker-Interface für einen Command (Schreiboperation).
/// </summary>
public interface ICommand;

/// <summary>
/// Marker-Interface für einen Command mit Rückgabewert.
/// </summary>
public interface ICommand<TResult>;

