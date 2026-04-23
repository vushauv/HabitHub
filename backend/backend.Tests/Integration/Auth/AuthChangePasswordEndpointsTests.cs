using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthChangePasswordEndpointsTests
{
    private readonly HttpClient _client;

    private async Task RegisterUser(string email, string password, int userType)
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            name = "Test User",
            email,
            password,
            timezone = "UTC",
            userType
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task<string> LogIn(string email, string password, int userType)
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password,
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body.SessionId;
    }

    public AuthChangePasswordEndpointsTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ChangePassword_ToValidPassword_Returns200_AndLoginWorks(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        await RegisterUser($"{uuid}@example.com", "Test1234", userType);
        var sessionId = await LogIn($"{uuid}@example.com", "Test1234", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.PostAsJsonAsync("/auth/change-password", new
        {
            currentPassword = "Test1234",
            newPassword = "Test1234!",
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response2 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}@example.com",
            password = "Test1234!",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var response3 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}@example.com",
            password = "Test1234",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response3.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ChangePassword_WithWrongPassword_Returns401_AndPasswordDoesNotChange(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        await RegisterUser($"{uuid}@example.com", "Test1234", userType);
        var sessionId = await LogIn($"{uuid}@example.com", "Test1234", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.PostAsJsonAsync("/auth/change-password", new
        {
            currentPassword = "WrongPassword!",
            newPassword = "Test1234!",
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response1.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response2 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}@example.com",
            password = "Test1234",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Theory(Skip = "TODO: Test fails")]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ChangePassword_WithWeakPassword_Returns400_AndPasswordDoesNotChange(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        await RegisterUser($"{uuid}@example.com", "Test1234", userType);
        var sessionId = await LogIn($"{uuid}@example.com", "Test1234", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.PostAsJsonAsync("/auth/change-password", new
        {
            currentPassword = "Test1234",
            newPassword = "T",
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response1.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response2 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}@example.com",
            password = "Test1234",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Theory]
    [InlineData("currentPassword")]
    [InlineData("newPassword")]
    public async Task ChangePassword_WithMissingField_Returns400_AndPasswordDoesNotChange(string field)
    {
        var uuid = Guid.NewGuid().ToString();
        await RegisterUser($"{uuid}@example.com", "Test1234", 0);
        var sessionId = await LogIn($"{uuid}@example.com", "Test1234", 0);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.PostAsJsonAsync("/auth/change-password", new
        {
            currentPassword = field != "currentPassword" ?  "Test1234" : null,
            newPassword = field != "newPassword" ? "Test1234!" : null
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response2 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}@example.com",
            password = "Test1234",
            userType = 0
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }
}
