namespace Handwerker.Application.Abstractions;

/// <summary>
/// Leitet Commands an den zuständigen Handler weiter — ohne MediatR.
/// </summary>
public interface ICommandDispatcher
{
    Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;
}

