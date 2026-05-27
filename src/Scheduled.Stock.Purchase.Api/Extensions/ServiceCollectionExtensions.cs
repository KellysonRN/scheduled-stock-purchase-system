using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Scheduled.Stock.Purchase.Api.Abstractions;

namespace Scheduled.Stock.Purchase.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.DefinedTypes
            .Where(type => type.IsClass && !type.IsAbstract)
            .Select(type => new
            {
                ImplementationType = type.AsType(),
                HandlerInterface = type.GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<,>))
            })
            .Where(x => x.HandlerInterface is not null)
            .ToList();

        foreach (var candidate in handlerTypes)
        {
            services.AddTransient(candidate.HandlerInterface!, candidate.ImplementationType);
        }

        return services;
    }
}
