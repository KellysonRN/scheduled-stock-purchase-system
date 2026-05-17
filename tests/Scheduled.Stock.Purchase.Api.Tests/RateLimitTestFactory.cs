using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Scheduled.Stock.Purchase.Api.Middleware;

namespace Scheduled.Stock.Purchase.Api.Tests;

/// <summary>
/// Custom WebApplicationFactory for rate limit integration testing.
/// Provides test-only endpoints for isolation via middleware.
/// </summary>
public class RateLimitTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // No additional services needed for test endpoints
        });

        builder.Configure(app =>
        {
            app.UseMiddleware<RateLimitingMiddleware>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Endpoint fake A
                endpoints.MapGet(
                    "/endpoint-a",
                    async context =>
                    {
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsJsonAsync(
                            new { endpoint = "endpoint-a", timestamp = DateTime.UtcNow }
                        );
                    }
                );

                // Endpoint fake B
                endpoints.MapGet(
                    "/endpoint-b",
                    async context =>
                    {
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsJsonAsync(
                            new { endpoint = "endpoint-b", timestamp = DateTime.UtcNow }
                        );
                    }
                );

                endpoints
                    .MapPost(
                        "/endpoint-a",
                        async context =>
                        {
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsJsonAsync(
                                new
                                {
                                    endpoint = "endpoint-a",
                                    method = "POST",
                                    timestamp = DateTime.UtcNow,
                                }
                            );
                        }
                    )
                    .WithName("EndpointAPost");
            });
        });

        base.ConfigureWebHost(builder);
    }
}
