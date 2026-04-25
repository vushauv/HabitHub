using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthChangeEmailEndpointTests
{
    private readonly HttpClient _client;

    public AuthChangeEmailEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ChangeEmail_ToUnusedEmail_Returns200_AndLoginWorks(int userType)
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
        var sessionId = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid1}@example.com",
            "Test1234!",
            "UTC",
            userType);
        await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid2}@example.com",
            "Test1234!",
            "UTC",
            userType);
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
        var sessionId = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}-a@example.com",
            "Test1234!",
            "UTC",
            userTypeA);
        await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}-b@example.com",
            "Test1234!",
            "UTC",
            userTypeB);
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
        var sessionId = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}@example.com",
            "Test1234!",
            "UTC",
            userType);
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
        var sessionId = await TestUtils.AuthRegister(
            _client,
            "Test User",
            $"{uuid}@example.com",
            "Test1234!",
            "UTC",
            0);
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
