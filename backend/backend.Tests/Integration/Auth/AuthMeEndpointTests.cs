using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using backend.Dtos.AuthDtos;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthMeEndpointTests
{
    private readonly HttpClient _client;

    public AuthMeEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task RegisterUser(string name, string email, string timezone, int userType)
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            name,
            email,
            password = "Test1234!",
            timezone,
            userType
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
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

    [Fact(Skip = "TODO: Enable test and implement endpoint")]
    public async Task GetMe_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync("/auth/me", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory(Skip = "TODO: Enable test and implement endpoint")]
    [InlineData(0)]
    [InlineData(1)]
    public async Task GetMe_Returns200_AndCorrectData(int userType)
    {
        // Generate user data
        var email = $"{Guid.NewGuid().ToString()}@example.com";
        var name = Guid.NewGuid().ToString();
        const string timezone = "UTC";
        
        // Register user
        await RegisterUser(name, email, timezone, userType);
        
        // Log in as new user
        var sessionId = await LogIn(email, userType);
        
        // Set session header
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        // Get /auth/me
        var response = await _client.GetAsync("/auth/me", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Remove session header
        _client.DefaultRequestHeaders.Remove("X-Session-Id");

        // Read the body and ensure that the password is not there
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("Test1234!", body);

        // Verify that all user fields are correct
        var user = JsonSerializer.Deserialize<UserDto>(body);
        Assert.NotNull(user);
        Assert.Equal(name, user.Name);
        Assert.Equal(email, user.Email);
        Assert.Equal(timezone, user.Timezone);
        Assert.Equal(userType, (int)user.UserType);
    }
}
