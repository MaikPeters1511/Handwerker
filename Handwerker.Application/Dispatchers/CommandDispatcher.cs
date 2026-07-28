using Handwerker.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Handwerker.Application.Dispatchers;

internal sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        return handler.HandleAsync(command, cancellationToken);
    }

    public Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return handler.HandleAsync(command, cancellationToken);
    }
}

