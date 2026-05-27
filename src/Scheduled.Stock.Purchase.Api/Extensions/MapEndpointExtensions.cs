using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Scheduled.Stock.Purchase.Api.Abstractions;

namespace Scheduled.Stock.Purchase.Api.Extensions;

internal static class MapEndpointExtensions
{
    public static IServiceCollection RegisterApiEndpointsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var endpointTypes = assembly.DefinedTypes
            .Where(type => type.IsClass && !type.IsAbstract && type.ImplementedInterfaces.Contains(typeof(IApiEndpoint)))
            .Select(type => type.AsType());

        foreach (var endpointType in endpointTypes)
        {
            services.AddTransient(typeof(IApiEndpoint), endpointType);
        }

        return services;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IApiEndpoint>>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
