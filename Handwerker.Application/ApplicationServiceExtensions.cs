using Handwerker.Application.Abstractions;
using Handwerker.Application.Dispatchers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Handwerker.Application;

public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Registriert alle CQRS-Handler der Application-Assembly sowie die Dispatcher.
    /// </summary>
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        var assembly = typeof(ApplicationServiceExtensions).Assembly;
        RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));
        RegisterHandlers(services, assembly, typeof(ICommandHandler<>));
        RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, Type openInterface)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterface)
                .Select(i => (Implementation: t, Interface: i)));

        foreach (var (impl, iface) in handlerTypes)
        {
            services.AddScoped(iface, impl);
        }
    }
}

