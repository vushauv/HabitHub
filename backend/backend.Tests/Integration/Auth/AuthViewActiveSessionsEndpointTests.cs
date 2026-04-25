using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthViewActiveSessionsEndpointTests
{
    private readonly HttpClient _client;
    public AuthViewActiveSessionsEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ViewActiveSessions_ReturnsOneSession(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        var sessionId = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}@example.com",
            "Test1234!",
            "UTC",
            userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<SessionDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ViewActiveSessions_AfterLoggingInTwice_ReturnsTwoSessions(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}@example.com",
            "Test1234!",
            "UTC",
            userType);
        var sessionId = await TestUtils.AuthLogIn(
            _client,
            $"{uuid}@example.com",
            "Test1234!",
            userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<SessionDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
    }

    [Fact]
    public async Task ViewActiveSessions_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}