using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthLoginEndpointTests : IClassFixture<AuthLoginEndpointTests.DatabaseInitializer>
{
    [Collection("Web app collection")]
    public class DatabaseInitializer
    {
        private readonly HttpClient _client;

        public DatabaseInitializer(TestWebAppFactory factory)
        {
            _client = factory.CreateClient();
            SeedUsers().GetAwaiter().GetResult();
        }

        private async Task SeedUsers()
        {
            var response1 = await _client.PostAsJsonAsync("/auth/register", new
            {
                name = "Test User",
                email = "infra-test-login-typecreator@example.com",
                password = "Test1234!",
                timezone = "UTC",
                userType = UserType.Creator
            });
            var response2 = await _client.PostAsJsonAsync("/auth/register", new
            {
                name = "Test User",
                email = "infra-test-login-typemember@example.com",
                password = "Test1234!",
                timezone = "UTC",
                userType = UserType.Member
            });
            var response3 = await _client.PostAsJsonAsync("/auth/register", new
            {
                name = "Test User",
                email = "infra-test-login-shared@example.com",
                password = "Test1234!",
                timezone = "UTC",
                userType = UserType.Creator
            });
            var response4 = await _client.PostAsJsonAsync("/auth/register", new
            {
                name = "Test User",
                email = "infra-test-login-shared@example.com",
                password = "Test1234!",
                timezone = "UTC",
                userType = UserType.Member
            });

            Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
            Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
            Assert.Equal(HttpStatusCode.Created, response3.StatusCode);
            Assert.Equal(HttpStatusCode.Created, response4.StatusCode);
        }
    }

    private readonly HttpClient _client;

    public AuthLoginEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(UserType.Creator)]
    [InlineData(UserType.Member)]
    public async Task Login_WithValidData_Returns200AndSessionForCorrectUser(UserType userType)
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"infra-test-login-type{userType.ToString().ToLowerInvariant()}@example.com",
            password = "Test1234!",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.NotNull(body.User);
        Assert.Equal($"infra-test-login-type{userType.ToString().ToLowerInvariant()}@example.com", body.User.Email);
        Assert.Equal(userType, body.User.UserType);
    }

    [Theory]
    [InlineData(UserType.Creator)]
    [InlineData(UserType.Member)]
    public async Task Login_WithEmailSharedBetweenTwoAccounts_Returns200AndSessionForCorrectUser(UserType userType)
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"infra-test-login-shared@example.com",
            password = "Test1234!",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.NotNull(body.User);
        Assert.Equal("infra-test-login-shared@example.com", body.User.Email);
        Assert.Equal(userType, body.User.UserType);
    }

    [Theory]
    [InlineData(UserType.Creator, UserType.Member)]
    [InlineData(UserType.Member, UserType.Creator)]
    public async Task Login_WithUserTypeMismatch_Returns401(UserType userTypeA, UserType userTypeB)
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"infra-test-login-type{userTypeA}@example.com",
            password = "Test1234!",
            userType = userTypeB
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingUserType_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "infra-test-login-shared@example.com",
            password = "Test1234!",
            //userType = UserType.Creator
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("email")]
    [InlineData("password")]
    public async Task Login_WithMissingField_Returns400(string field)
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = field != "email" ? "infra-test-login-shared@example.com" : null,
            password = field != "password" ? "Test1234!" : null,
            userType = UserType.Creator
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
