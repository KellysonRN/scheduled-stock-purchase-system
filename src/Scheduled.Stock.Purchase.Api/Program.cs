using Scalar.AspNetCore;

using Scheduled.Stock.Purchase.Api;
using Scheduled.Stock.Purchase.Api.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi();

WebApplication app = builder.Build();

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

string[] summaries = new[]
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

if (app.Environment.IsDevelopment())
{
    app.MapGet(
        "/test",
        () =>
        {
            throw new NotImplementedException("Teste");
        }
    );
}

app.MapGet(
        "/weatherforecast",
        () =>
        {
            WeatherForecast[] forecast = Enumerable
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

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
