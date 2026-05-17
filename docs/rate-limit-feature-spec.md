# Rate Limit Feature Specification

## Overview

This document describes the current rate limiting behavior implemented in `RateLimitingMiddleware`.

- Allows up to **2 requests**
- Within a **10-second window**
- Per **same client**
- Per **same endpoint**
- Per **same HTTP method**
- Enforced on request identity defined by `clientIdentifier:HTTP_METHOD:PATH`

---

## Behavior

### Client identification

The middleware uses a priority-based identification strategy:

1. **X-User-Id header** (explicit user identification) - Primary
   - Format: `user:{userId}`
   - Example: `X-User-Id: user-123`
2. **X-Forwarded-For header** (proxy scenario) - Secondary
   - Takes the first IP from the comma-separated list
   - Format: `ip:{ipAddress}`
3. **RemoteIpAddress** (direct connection) - Tertiary
   - Format: `ip:{ipAddress}`
4. **Fallback** (neither header nor IP available)
   - Format: `unknown:client`

### Endpoint key

- Builds a unique key as:
  - `clientIdentifier + ":" + HTTP_METHOD + ":" + requestPath`
- Rate limiting is enforced separately for:
  - each client
  - each HTTP method
  - each request path

### Window and counting

- The rate limit window is **10 seconds**
- The allowed request count is **2**
- When the window expires, the counter resets
- Requests after the window reset start a new window

### Rejection behavior

- If a client exceeds the limit for the same endpoint and method, the middleware:
  - returns **HTTP 429 Too Many Requests**
  - sets header `Retry-After` with remaining seconds
  - writes response body: `Too many requests. Please try again later.`

---

## Acceptance criteria

### Scenario 1: Allow up to two requests within 10 seconds

Given a client makes a request to `GET /endpoint-a`
When the client sends a second request to the same endpoint within 10 seconds
Then both requests are allowed

### Scenario 2: Reject the third request within the same 10-second window

Given a client has already sent two requests to `GET /endpoint-a`
When the client sends a third request to `GET /endpoint-a` within 10 seconds
Then the middleware returns `429 Too Many Requests`
And `Retry-After` is set to a value between 1 and 10 seconds
And the response body contains `Too many requests. Please try again later.`

### Scenario 3: Reset limit after 10 seconds

Given a client sent two requests to `GET /endpoint-a`
And 10 seconds have passed since the first request window started
When the client sends another request to `GET /endpoint-a`
Then the new request is allowed

### Scenario 4: Separate limits per endpoint

Given a client sends two requests to `GET /endpoint-a`
When the same client sends a request to `GET /endpoint-b`
Then the request is allowed because endpoint identity is different

### Scenario 5: Separate limits per client/user

Given one client sends two requests to `GET /endpoint-a`
When a second client sends a request to `GET /endpoint-a`
Then the second client request is allowed

### Scenario 6: Separate limits by HTTP method

Given a client sends two `GET` requests to `/endpoint-a`
When the same client sends a `POST` request to `/endpoint-a`
Then the request is allowed because the HTTP method differs

### Scenario 7: User identification via X-User-Id header

Given a client sends `X-User-Id: user-123` header
When the client makes requests
Then the rate limit is applied to `user:user-123` identifier
And requests from `user-124` are treated separately

### Scenario 8: Fallback to IP when no user header exists

Given no `X-User-Id` header is provided
When a client sends requests from the same IP to `GET /endpoint-a`
Then rate limiting is applied against the IP-based identifier
And the third request is rejected with `429`

### Scenario 9: Exact boundary enforcement

Given a client sends two requests to `GET /endpoint-a`
When the client sends a third and fourth request to the same endpoint within 10 seconds
Then the third and fourth requests are rejected with `429`

---

## Integration Testing

### Setup: WebApplicationFactory

The test suite uses `RateLimitTestFactory` extending `WebApplicationFactory<Program>` to:

- Provide isolated HTTP client instances
- Start the full ASP.NET Core pipeline under test
- Ensure clean state before each test via `RateLimitingMiddleware.ClearState()`

### Test endpoint registration

Test endpoints are registered conditionally in `src/Scheduled.Stock.Purchase.Api/Program.cs` when the app environment is set to `Testing`.
This ensures the endpoints exist only for integration tests and do not pollute production routing.

### Test Framework

- **xUnit**: Test framework
- **Shouldly**: Fluent assertion library
- **WebApplicationFactory**: Full integration testing with real HTTP pipeline

### Test Classes

#### RateLimitTestFactory

Located in: `tests/Scheduled.Stock.Purchase.Api.Tests/RateLimitTestFactory.cs`

- Extends `WebApplicationFactory<Program>`
- Uses the application’s `Testing` environment to activate test-only endpoints
- Does not register endpoints directly in the factory

#### RateLimitIntegrationTests

Located in: `tests/Scheduled.Stock.Purchase.Api.Tests/RateLimitIntegrationTests.cs`

- Contains 9 integration test scenarios (Scenario1 through Scenario9)
- Each test:
  - Calls `InitializeAsync()` to create a fresh factory and clear state
  - Executes HTTP requests with explicit user or IP identification
  - Validates HTTP status codes and `Retry-After` headers using Shouldly
  - Calls `DisposeAsync()` to clean up resources

### Running Tests

```bash
# Run all rate limit tests
dotnet test tests/Scheduled.Stock.Purchase.Api.Tests/Scheduled.Stock.Purchase.Api.Tests.csproj

# Run specific test scenario
dotnet test tests/Scheduled.Stock.Purchase.Api.Tests/Scheduled.Stock.Purchase.Api.Tests.csproj -k Scenario1

# Run with coverage
dotnet test tests/Scheduled.Stock.Purchase.Api.Tests/Scheduled.Stock.Purchase.Api.Tests.csproj /p:CollectCoverage=true
```

### Example: User Request Flow in Tests

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, "/endpoint-a");
request.Headers.Add("X-User-Id", "user-123");

// Middleware builds key: "user:user-123:GET:/endpoint-a"
var response = await client.SendAsync(request);
response.StatusCode.ShouldBe(HttpStatusCode.OK);
```

---

## Production Recommendations

### 1. Distributed Rate Limiting with Redis

For multi-instance deployments, implement an abstract `IDistributedRateLimiter`:

```csharp
public interface IDistributedRateLimiter
{
    Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan window);
    Task ResetAsync(string key);
}

public class RedisRateLimiter : IDistributedRateLimiter
{
    private readonly IConnectionMultiplexer _redis;

    public async Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan window)
    {
        var db = _redis.GetDatabase();
        var count = await db.StringIncrementAsync(key);

        if (count == 1)
        {
            await db.KeyExpireAsync(key, window);
        }

        return count <= maxRequests;
    }
}
```

Replace in-memory `ConcurrentDictionary` with Redis for shared state across multiple instances.

### 2. Observability with OpenTelemetry

Add structured logging and metrics:

```csharp
public class RateLimitingMiddleware
{
    private static readonly ActivitySource ActivitySource = new("RateLimitingMiddleware");
    private static readonly Counter<long> RateLimitCounter = Meter.CreateCounter<long>("ratelimit.requests");

    public async Task InvokeAsync(HttpContext context)
    {
        using var activity = ActivitySource.StartActivity("RateLimit");
        activity?.SetTag("user", clientId);
        activity?.SetTag("endpoint", endpoint);

        if (shouldReject)
        {
            activity?.SetTag("rate_limit.exceeded", true);
            RateLimitCounter.Add(1, new("status", "rejected"));
        }
        else
        {
            RateLimitCounter.Add(1, new("status", "allowed"));
        }
    }
}
```

#### Configuration in Program.cs

```csharp
builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());
```

### 3. Configuration-Based Rate Limits

Make rate limits configurable:

```csharp
public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 2;
    public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(10);
    public bool Enabled { get; set; } = true;
}

builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection("RateLimit"));
```

### 4. User-Specific Limits

Allow different limits for different user tiers:

```csharp
public interface IUserLimitProvider
{
    Task<(int maxRequests, TimeSpan window)> GetLimitAsync(string userId);
}


public class TieredUserLimitProvider : IUserLimitProvider
{
    public async Task<(int, TimeSpan)> GetLimitAsync(string userId)
    {
        var user = await _userService.GetAsync(userId);
        return user.Tier switch
        {
            UserTier.Free => (2, TimeSpan.FromSeconds(10)),
            UserTier.Premium => (100, TimeSpan.FromSeconds(60)),
            UserTier.Enterprise => (int.MaxValue, TimeSpan.Zero),
            _ => (2, TimeSpan.FromSeconds(10))
        };
    }
}

```

### 5. Monitoring & Alerting

Track rate limit metrics in APM tools (Application Insights, Datadog, etc.):

- **Metric**: Count of 429 responses per hour
- **Alert**: If 429 count exceeds threshold (e.g., 10% of total requests)
- **Dashboard**: Visualize rate limit rejections by endpoint, user tier, time

---

## Implementation Notes

- The middleware stores per-client-per-endpoint counters in a shared `ConcurrentDictionary`
- Expired entries are cleaned up when a new window starts for a given key
- The 10-second window is based on `DateTime.UtcNow`
- User identification is extracted in priority order: `X-User-Id` → `X-Forwarded-For` → `RemoteIpAddress`
- This specification documents the current in-memory implementation suitable for single-instance deployments
- For production multi-instance deployments, use Redis or other distributed stores (see Recommendations)
