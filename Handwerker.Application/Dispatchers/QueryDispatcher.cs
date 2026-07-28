using Handwerker.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Handwerker.Application.Dispatchers;

internal sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return handler.HandleAsync(query, cancellationToken);
    }
}

