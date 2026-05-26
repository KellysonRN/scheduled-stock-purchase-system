using Scheduled.Stock.Purchase.Api.Middleware;

namespace Scheduled.Stock.Purchase.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddProblemDetails();
        services.AddExceptionHandler<ExceptionMiddleware>();

        return services;
    }
}
