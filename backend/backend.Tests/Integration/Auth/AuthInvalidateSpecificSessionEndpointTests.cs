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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task InvalidateSpecificSession_InvalidatesSession(int userType)
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
        var sessionIdA = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}-a@example.com",
            "Test1234!",
            "UTC",
            userType);
        var sessionIdB = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}-b@example.com",
            "Test1234!",
            "UTC",
            userType);
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
        var sessionId = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}@example.com",
            "Test1234!",
            "UTC",
            userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response2 = await _client.DeleteAsync($"/auth/sessions/notreal", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
        
        var response3 = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
    }
}