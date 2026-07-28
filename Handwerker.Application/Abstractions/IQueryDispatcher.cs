namespace Handwerker.Application.Abstractions;

/// <summary>
/// Leitet Queries an den zuständigen Handler weiter — ohne MediatR.
/// </summary>
public interface IQueryDispatcher
{
    Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}

