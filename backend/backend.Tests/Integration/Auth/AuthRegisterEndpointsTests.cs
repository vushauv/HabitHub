using System.Net;
using System.Net.Http.Json;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthRegisterEndpointsTests
{
    private readonly HttpClient _client;

    public AuthRegisterEndpointsTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            name = "Test User",
            email = "infra-test@example.com",
            password = "Test1234!",
            timezone = "UTC",
            userType = 0  // UserType.Creator
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidData_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            invalidField = "Test",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingUserType_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            name = "Test User",
            email = "infra-test-missing-type@example.com",
            password = "Test1234!",
            timezone = "UTC",
            //userType = 0  // UserType.Creator
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("email")]
    [InlineData("password")]
    [InlineData("timezone")]
    public async Task Register_WithMissingField_Returns400(string field)
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            name = field != "name" ? "Test User" : null,
            email = field != "email" ? "infra-test-missing@example.com" : null,
            password = field != "password" ? "Test1234!" : null,
            timezone = field != "timezone" ? "UTC" : null,
            userType = 0  // UserType.Creator
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_BothUserTypesWithSameEmail_Returns201()
    {
        var response1 = await _client.PostAsJsonAsync("/auth/register", new
        {
            name = "Test User",
            email = "infra-test-2types@example.com",
            password = "Test1234!",
            timezone = "UTC",
            userType = 0  // UserType.Creator
        }, TestContext.Current.CancellationToken);
        var response2 = await _client.PostAsJsonAsync("/auth/register", new
        {
            name = "Test User",
            email = "infra-test-2types@example.com",
            password = "Test1234!",
            timezone = "UTC",
            userType = 1  // UserType.Member
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
    }

    [Theory]
	[InlineData(0)]
	[InlineData(1)]
    public async Task Register_TwiceSameUserTypeWithSameEmail_Returns409(int userType)
    {
        var response1 = await _client.PostAsJsonAsync("/auth/register", new
        {
            name = "Test User",
            email = "infra-test-econflict@example.com",
            password = "Test1234!",
            timezone = "UTC",
            userType = userType
        }, TestContext.Current.CancellationToken);
        var response2 = await _client.PostAsJsonAsync("/auth/register", new
        {
            name = "Test User",
            email = "infra-test-econflict@example.com",
            password = "Test1234!",
            timezone = "UTC",
            userType = userType
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }
}
