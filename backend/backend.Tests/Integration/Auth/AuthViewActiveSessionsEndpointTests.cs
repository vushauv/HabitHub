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

    private async Task<string> RegisterUser(string email, int userType)
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            name = "Test User",
            email,
            password = "Test1234!",
            timezone = "UTC",
            userType
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body.SessionId;
    }

    private async Task<string> LogIn(string email, int userType)
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "Test1234!",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body.SessionId;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ViewActiveSessions_ReturnsOneSession(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        var sessionId = await RegisterUser($"{uuid}@example.com", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<SessionDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ViewActiveSessions_AfterLoggingInTwice_ReturnsTwoSessions(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        await RegisterUser($"{uuid}@example.com", userType);
        var sessionId = await LogIn($"{uuid}@example.com", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<SessionDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
    }

    [Fact]
    public async Task ViewActiveSessions_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}