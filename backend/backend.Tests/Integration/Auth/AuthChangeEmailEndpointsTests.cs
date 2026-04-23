using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthChangeEmailEndpointsTests
{
    private readonly HttpClient _client;

    private async Task RegisterUser(string email, int userType)
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

    public AuthChangeEmailEndpointsTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ChangeEmail_ToUnusedEmail_Returns200_AndLoginWorks(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        await RegisterUser($"{uuid}@example.com", userType);
        var sessionId = await LogIn($"{uuid}@example.com", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.PostAsJsonAsync("/auth/change-email", new
        {
            newEmail = $"{uuid}-new@example.com",
            password = "Test1234!"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response2 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}-new@example.com",
            password = "Test1234!",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var response3 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}@example.com",
            password = "Test1234!",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response3.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ChangeEmail_ToUsedEmail_Returns409_AndEmailDoesNotChange(int userType)
    {
        var uuid1 = Guid.NewGuid().ToString();
        var uuid2 = Guid.NewGuid().ToString();
        await RegisterUser($"{uuid1}@example.com", userType);
        await RegisterUser($"{uuid2}@example.com", userType);
        var sessionId = await LogIn($"{uuid1}@example.com", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.PostAsJsonAsync("/auth/change-email", new
        {
            newEmail = $"{uuid2}@example.com",
            password = "Test1234!"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, response1.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response2 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid1}@example.com",
            password = "Test1234!",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public async Task ChangeEmail_ToEmailUsedByOtherUserType_Returns200_AndLoginWorks(int userTypeA, int userTypeB)
    {
        var uuid = Guid.NewGuid().ToString();
        await RegisterUser($"{uuid}-a@example.com", userTypeA);
        await RegisterUser($"{uuid}-b@example.com", userTypeB);
        var sessionId = await LogIn($"{uuid}-a@example.com", userTypeA);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.PostAsJsonAsync("/auth/change-email", new
        {
            newEmail = $"{uuid}-b@example.com",
            password = "Test1234!"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response2 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}-b@example.com",
            password = "Test1234!",
            userType = userTypeA
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var response3 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}-a@example.com",
            password = "Test1234!",
            userType = userTypeA
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response3.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ChangeEmail_WithWrongPassword_Returns401_AndEmailDoesNotChange(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        await RegisterUser($"{uuid}@example.com", userType);
        var sessionId = await LogIn($"{uuid}@example.com", userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.PostAsJsonAsync("/auth/change-email", new
        {
            newEmail = $"{uuid}-new@example.com",
            password = "WrongPassword!"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response1.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response2 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}@example.com",
            password = "Test1234!",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Theory]
    [InlineData("newEmail")]
    [InlineData("password")]
    public async Task ChangeEmail_WithMissingField_Returns400_AndEmailDoesNotChange(string field)
    {
        var uuid = Guid.NewGuid().ToString();
        await RegisterUser($"{uuid}@example.com", 0);
        var sessionId = await LogIn($"{uuid}@example.com", 0);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.PostAsJsonAsync("/auth/change-email", new
        {
            newEmail = field != "newEmail" ?  $"{uuid}-new@example.com" : null,
            password = field != "password" ? "Test1234!" : null
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response2 = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"{uuid}@example.com",
            password = "Test1234!",
            userType = 0
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }
}
