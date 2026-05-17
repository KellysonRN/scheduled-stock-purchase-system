using Scheduled.Stock.Purchase.Api.Middleware;
using Shouldly;

namespace Scheduled.Stock.Purchase.Api.Tests;

/// <summary>
/// Integration tests for RateLimitMiddleware using WebApplicationFactory and Shouldly.
/// Tests user identification via X-User-Id header and rate limiting behavior.
///
/// Each test clears rate limit state before execution to ensure isolation.
/// </summary>
public class RateLimitIntegrationTests : IAsyncLifetime
{
    private RateLimitTestFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _factory = new RateLimitTestFactory();
        _client = _factory.CreateClient();
        // Ensure clean state before each test
        RateLimitingMiddleware.ClearState();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        RateLimitingMiddleware.ClearState();
        _client?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Scenario 1: Allows up to 2 requests from the same user to the same endpoint within the window.
    /// </summary>
    [Fact]
    public async Task Scenario1_AllowsTwoRequestsSameUserSameEndpointWithinWindow()
    {
        // Arrange
        const string userId = "user-123";
        var request1 = CreateRequest("GET", "/endpoint-a", userId);
        var request2 = CreateRequest("GET", "/endpoint-a", userId);

        // Act
        var response1 = await _client.SendAsync(request1);
        var response2 = await _client.SendAsync(request2);

        // Assert
        response1.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Scenario 2: Blocks the 3rd request with HTTP 429 and includes Retry-After header.
    /// </summary>
    [Fact]
    public async Task Scenario2_BlocksThirdRequestWithStatus429AndRetryAfterHeader()
    {
        // Arrange
        const string userId = "user-456";
        var request1 = CreateRequest("GET", "/endpoint-a", userId);
        var request2 = CreateRequest("GET", "/endpoint-a", userId);
        var request3 = CreateRequest("GET", "/endpoint-a", userId);

        // Act
        var response1 = await _client.SendAsync(request1);
        var response2 = await _client.SendAsync(request2);
        var response3 = await _client.SendAsync(request3);

        // Assert - First two requests allowed
        response1.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

        // Third request blocked
        response3.StatusCode.ShouldBe(System.Net.HttpStatusCode.TooManyRequests);
        response3.Headers.RetryAfter.ShouldNotBeNull();

        // Verify response body
        var content = await response3.Content.ReadAsStringAsync();
        content.ShouldContain("Too many requests");
    }

    /// <summary>
    /// Scenario 3: Allows separate limits per endpoint (same user can have 2 requests per endpoint).
    /// </summary>
    [Fact]
    public async Task Scenario3_AllowsSeparateLimitsPerEndpoint()
    {
        // Arrange
        const string userId = "user-789";

        // Act: Send 2 requests to endpoint-a
        var req1 = CreateRequest("GET", "/endpoint-a", userId);
        var req2 = CreateRequest("GET", "/endpoint-a", userId);
        var resp1 = await _client.SendAsync(req1);
        var resp2 = await _client.SendAsync(req2);

        // Send 1 request to endpoint-b (different endpoint, should be allowed)
        var req3 = CreateRequest("GET", "/endpoint-b", userId);
        var resp3 = await _client.SendAsync(req3);

        // Assert
        resp1.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        resp2.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        resp3.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK); // Different endpoint = separate limit
    }

    /// <summary>
    /// Scenario 4: Allows separate limits per user (different users don't share limits).
    /// </summary>
    [Fact]
    public async Task Scenario4_AllowsSeparateLimitsPerUser()
    {
        // Arrange & Act: User A sends 2 requests to endpoint-a
        var user1Req1 = CreateRequest("GET", "/endpoint-a", "user-a");
        var user1Req2 = CreateRequest("GET", "/endpoint-a", "user-a");
        var user1Resp1 = await _client.SendAsync(user1Req1);
        var user1Resp2 = await _client.SendAsync(user1Req2);

        // User B sends 1 request to the same endpoint (should be allowed)
        var user2Req = CreateRequest("GET", "/endpoint-a", "user-b");
        var user2Resp = await _client.SendAsync(user2Req);

        // Assert
        user1Resp1.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        user1Resp2.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        user2Resp.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK); // Different user = separate limit
    }

    /// <summary>
    /// Scenario 5: Resets the limit after 10 seconds (requires system clock observation).
    /// Note: This test uses real time delays. Consider using TestTimeProvider abstraction for faster tests.
    /// </summary>
    [Fact(Skip = "Skipped by default (10s delay). Enable to test window reset with real time.")]
    public async Task Scenario5_ResetsLimitAfter10Seconds()
    {
        // Arrange
        const string userId = "user-reset";

        // Act: Send 2 requests
        var req1 = CreateRequest("GET", "/endpoint-a", userId);
        var req2 = CreateRequest("GET", "/endpoint-a", userId);
        await _client.SendAsync(req1);
        await _client.SendAsync(req2);

        // Wait for window to expire
        await Task.Delay(TimeSpan.FromSeconds(10.5));

        // Send another request (should be allowed as window expired)
        var req3 = CreateRequest("GET", "/endpoint-a", userId);
        var resp3 = await _client.SendAsync(req3);

        // Assert
        resp3.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK); // Should allow after window reset
    }

    /// <summary>
    /// Scenario 6: Validates Retry-After header contains valid numeric value between 1 and 10.
    /// </summary>
    [Fact]
    public async Task Scenario6_ValidatesRetryAfterHeaderValue()
    {
        // Arrange
        const string userId = "user-retry";
        var req1 = CreateRequest("GET", "/endpoint-a", userId);
        var req2 = CreateRequest("GET", "/endpoint-a", userId);
        var req3 = CreateRequest("GET", "/endpoint-a", userId);

        // Act
        await _client.SendAsync(req1);
        await _client.SendAsync(req2);
        var response = await _client.SendAsync(req3);

        // Assert
        response.Headers.RetryAfter.ShouldNotBeNull();
        var retryAfterValue =
            response.Headers.RetryAfter!.Delta?.TotalSeconds.ToString()
            ?? response.Headers.RetryAfter!.Date?.ToString();
        retryAfterValue.ShouldNotBeNullOrEmpty();

        // Should be parseable as integer (from Delta)
        if (response.Headers.RetryAfter.Delta.HasValue)
        {
            var seconds = (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds;
            seconds.ShouldBeGreaterThan(0);
            seconds.ShouldBeLessThanOrEqualTo(10);
        }
    }

    /// <summary>
    /// Scenario 7: Separates limits by HTTP method (GET vs POST on same path).
    /// </summary>
    [Fact]
    public async Task Scenario7_SeparatesLimitsByHttpMethod()
    {
        // Arrange
        const string userId = "user-method";

        // Act: Send 2 GET requests to /endpoint-a
        var get1 = CreateRequest("GET", "/endpoint-a", userId);
        var get2 = CreateRequest("GET", "/endpoint-a", userId);
        await _client.SendAsync(get1);
        await _client.SendAsync(get2);

        // Send 1 POST request to same path (should be allowed as method differs)
        var post1 = CreateRequest("POST", "/endpoint-a", userId);
        var postResp = await _client.SendAsync(post1);

        // Assert
        postResp.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK); // Different method = separate limit
    }

    /// <summary>
    /// Scenario 8: Falls back to IP address identification when X-User-Id is not provided.
    /// </summary>
    [Fact]
    public async Task Scenario8_AllowsRequestsWithoutUserIdHeaderUsingRemoteIp()
    {
        // Arrange: Create requests without X-User-Id (should fallback to IP)
        var req1 = CreateRequest("GET", "/endpoint-a", null);
        var req2 = CreateRequest("GET", "/endpoint-a", null);
        var req3 = CreateRequest("GET", "/endpoint-a", null);

        // Act
        var resp1 = await _client.SendAsync(req1);
        var resp2 = await _client.SendAsync(req2);
        var resp3 = await _client.SendAsync(req3);

        // Assert
        resp1.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        resp2.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        resp3.StatusCode.ShouldBe(System.Net.HttpStatusCode.TooManyRequests); // IP-based rate limit enforced
    }

    /// <summary>
    /// Scenario 9: Verifies exact boundary - exactly 2 allowed, 3rd and 4th blocked.
    /// </summary>
    [Fact]
    public async Task Scenario9_AllowsExactlyTwoRequestsAndBlocksOn3rdAnd4th()
    {
        // Arrange
        const string userId = "user-exact";

        // Act & Assert: Verify exact boundary
        var req1 = CreateRequest("GET", "/endpoint-a", userId);
        var resp1 = await _client.SendAsync(req1);
        resp1.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

        var req2 = CreateRequest("GET", "/endpoint-a", userId);
        var resp2 = await _client.SendAsync(req2);
        resp2.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

        var req3 = CreateRequest("GET", "/endpoint-a", userId);
        var resp3 = await _client.SendAsync(req3);
        resp3.StatusCode.ShouldBe(System.Net.HttpStatusCode.TooManyRequests);

        // Verify 4th is also blocked
        var req4 = CreateRequest("GET", "/endpoint-a", userId);
        var resp4 = await _client.SendAsync(req4);
        resp4.StatusCode.ShouldBe(System.Net.HttpStatusCode.TooManyRequests);
    }

    /// <summary>
    /// Helper method to create a request with optional X-User-Id header.
    /// </summary>
    private static HttpRequestMessage CreateRequest(string method, string path, string? userId)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (!string.IsNullOrEmpty(userId))
        {
            request.Headers.Add("X-User-Id", userId);
        }
        return request;
    }
}
