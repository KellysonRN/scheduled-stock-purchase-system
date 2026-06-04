using System.Reflection;
using Scalar.AspNetCore;
using Scheduled.Stock.Purchase.Api.Extensions;
using Scheduled.Stock.Purchase.Api.Middleware;
using Scheduled.Stock.Purchase.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionMiddleware>();

// Infrastructure
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddInfrastructure(connectionString);

builder.Services.RegisterApiEndpointsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddHandlersFromAssembly(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapApiEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
        options
            .WithTitle("Scheduled.Stock.Purchase.Api")
            .WithTheme(ScalarTheme.Default)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .WithOpenApiRoutePattern("/openapi/{documentName}.json")
            .SortTagsAlphabetically()
            .SortOperationsByMethod()
            .ExpandAllTags()
            .HideDeveloperTools()
    );
}

app.UseExceptionHandler();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseHttpsRedirection();

app.MapGet(
        "/weatherforecast",
        () =>
        {
            var summaries = new[]
            {
                "Freezing",
                "Bracing",
                "Chilly",
                "Cool",
                "Mild",
                "Warm",
                "Balmy",
                "Hot",
                "Sweltering",
                "Scorching",
            };

            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();

            return forecast;
        }
    )
    .WithName("GetWeatherForecast");

await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
