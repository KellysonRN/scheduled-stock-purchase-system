using System.Collections.Concurrent;
using Microsoft.Extensions.Primitives;

namespace Scheduled.Stock.Purchase.Api.Middleware;

/// <summary>
/// Rate limiting middleware that restricts requests per user and endpoint.
/// - Max 2 requests per user per endpoint per 10-second window
/// - User identification: X-User-Id header (preferred) or IP address (fallback)
/// - Returns HTTP 429 (Too Many Requests) when limit exceeded
/// </summary>
public sealed class RateLimitingMiddleware(RequestDelegate next)
{
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(10);
    private const int MaxRequests = 2;
    private static readonly ConcurrentDictionary<string, RequestCounter> Requests = new();

    public async Task InvokeAsync(HttpContext context)
    {
        var endpointKey = BuildKey(context);
        var now = DateTime.UtcNow;
        var counter = Requests.GetOrAdd(endpointKey, _ => new RequestCounter(now));

        bool shouldReject;
        TimeSpan retryAfter;

        lock (counter)
        {
            if (now - counter.WindowStart >= Window)
            {
                counter.WindowStart = now;
                counter.Count = 0;
            }

            counter.Count++;
            shouldReject = counter.Count > MaxRequests;
            retryAfter = counter.WindowStart + Window - now;

            if (counter.Count == 1)
            {
                CleanupExpiredEntries(now);
            }
        }

        if (shouldReject)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = Math.Max(1, (int)retryAfter.TotalSeconds)
                .ToString();
            await context.Response.WriteAsync("Too many requests. Please try again later.");
            return;
        }

        await next(context);
    }

    private static string BuildKey(HttpContext context)
    {
        var client = GetClientIdentifier(context);
        var endpoint =
            context.Request.Method.ToUpperInvariant()
            + ":"
            + (context.Request.Path.Value ?? string.Empty).ToLowerInvariant();
        return $"{client}:{endpoint}";
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Priority 1: X-User-Id header (explicit user identification)
        if (
            context.Request.Headers.TryGetValue("X-User-Id", out StringValues userIdValues)
            && userIdValues.Count > 0
        )
        {
            var userId = userIdValues.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }
        }

        // Priority 2: X-Forwarded-For header (proxy scenario)
        if (
            context.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedValues)
            && forwardedValues.Count > 0
        )
        {
            var forwardedHeader = forwardedValues.ToString();
            if (!string.IsNullOrWhiteSpace(forwardedHeader))
            {
                var firstForwarded = forwardedHeader.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrWhiteSpace(firstForwarded))
                {
                    return $"ip:{firstForwarded}";
                }
            }
        }

        // Priority 3: Remote IP address
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrWhiteSpace(remoteIp))
        {
            return $"ip:{remoteIp}";
        }

        return "unknown:client";
    }

    private static void CleanupExpiredEntries(DateTime now)
    {
        foreach (var item in Requests.ToList())
        {
            if (now - item.Value.WindowStart >= Window)
            {
                Requests.TryRemove(item.Key, out _);
            }
        }
    }

    /// <summary>
    /// Clears all rate limit state. Useful for testing and debugging.
    /// </summary>
    public static void ClearState() => Requests.Clear();

    private sealed class RequestCounter(DateTime windowStart)
    {
        public DateTime WindowStart { get; set; } = windowStart;
        public int Count { get; set; } = 0;
    }
}
