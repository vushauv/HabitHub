using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthLogoutEndpointTests
{
    private readonly HttpClient _client;
    public AuthLogoutEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Logout_WithoutSession_Returns400()
    {
        var response = await _client.DeleteAsync("/auth/logout", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithInvalidSession_Returns401()
    {
        _client.DefaultRequestHeaders.Add("X-Session-Id", "1234~!");
        var response = await _client.DeleteAsync("/auth/logout", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Logout_WithValidSession_Returns204_And_InvalidatesSession(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        var sessionId1 = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}@example.com",
            "Test1234!",
            "UTC",
            userType);
        var sessionId2 = await TestUtils.AuthLogIn(
            _client,
            $"{uuid}@example.com",
            "Test1234!",
            userType);
        
        
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId2);
        var response1 = await _client.DeleteAsync("/auth/logout", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        var response2 = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response2.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId1);
        var response3 = await _client.GetAsync("/auth/sessions", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var body = await response3.Content.ReadFromJsonAsync<List<SessionDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
    }
}