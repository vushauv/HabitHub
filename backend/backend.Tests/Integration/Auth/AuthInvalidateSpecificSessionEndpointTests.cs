using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthInvalidateSpecificSessionEndpointTests
{
    private readonly HttpClient _client;
    public AuthInvalidateSpecificSessionEndpointTests(TestWebAppFactory factory)
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
    public async Task InvalidateSpecificSession_InvalidatesSession(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        var sessionId = await RegisterUser($"{uuid}@example.com", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var body = await response1.Content.ReadFromJsonAsync<List<SessionDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
        
        var response2 = await _client.DeleteAsync($"/auth/sessions/{sessionId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        
        var response3 = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response3.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task InvalidateSpecificSession_OfOtherUser_DoesNotInvalidateSession(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        var sessionIdA = await RegisterUser($"{uuid}-a@example.com", userType);
        var sessionIdB = await RegisterUser($"{uuid}-b@example.com", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionIdA);
        
        var response1 = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var body = await response1.Content.ReadFromJsonAsync<List<SessionDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
        
        var response2 = await _client.DeleteAsync($"/auth/sessions/{sessionIdB}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
        
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionIdB);
        
        var response3 = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task InvalidateSpecificSession_WhichDoesNotExist_Returns404(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        var sessionId = await RegisterUser($"{uuid}@example.com", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response2 = await _client.DeleteAsync($"/auth/sessions/notreal", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
        
        var response3 = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
    }
}