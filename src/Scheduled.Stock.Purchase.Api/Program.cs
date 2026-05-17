using Scalar.AspNetCore;
using Scheduled.Stock.Purchase.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionMiddleware>();

var app = builder.Build();

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

// Test endpoints registration (only in tests via WebApplicationFactory override)
if (!app.Environment.IsProduction() && app.Environment.EnvironmentName == "Testing")
{
    app.MapGet("/endpoint-a", () => new { endpoint = "endpoint-a", timestamp = DateTime.UtcNow })
        .WithName("EndpointA");

    app.MapGet("/endpoint-b", () => new { endpoint = "endpoint-b", timestamp = DateTime.UtcNow })
        .WithName("EndpointB");

    app.MapPost("/endpoint-a", () => new { endpoint = "endpoint-a", method = "POST", timestamp = DateTime.UtcNow })
        .WithName("EndpointAPost");
}

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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
