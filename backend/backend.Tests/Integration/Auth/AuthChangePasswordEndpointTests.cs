using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthChangePasswordEndpointTests
{
    private readonly HttpClient _client;

    public AuthChangePasswordEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ChangePassword_ToValidPassword_Returns200_AndLoginWorks(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        var sessionId = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}@example.com",
            "Test1234",
            "UTC",
            userType);
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
        var sessionId = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}@example.com",
            "Test1234",
            "UTC",
            userType);
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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ChangePassword_WithWeakPassword_Returns400_AndPasswordDoesNotChange(int userType)
    {
        var uuid = Guid.NewGuid().ToString();
        var sessionId = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}@example.com",
            "Test1234",
            "UTC",
            userType);
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response1 = await _client.PostAsJsonAsync("/auth/change-password", new
        {
            currentPassword = "Test1234",
            newPassword = "T",
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);
        
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
        var sessionId = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}@example.com",
            "Test1234",
            "UTC",
            0);
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
