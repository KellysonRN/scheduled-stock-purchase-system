using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheduled.Stock.Purchase.Infrastructure.Data;
using Scheduled.Stock.Purchase.Infrastructure.Repositories;

namespace Scheduled.Stock.Purchase.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString
    )
    {
        // Database
        services
            .AddDbContext<ScheduledStockPurchaseDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            )
            .AddScoped<DbContext>(provider =>
                provider.GetRequiredService<ScheduledStockPurchaseDbContext>()
            );

        // Repositories
        services.AddScoped<ITradeRepository, TradeRepository>();

        return services;
    }
}
