using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthLoginEndpointsTests : IClassFixture<AuthLoginEndpointsTests.DatabaseInitializer>
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
                email = "infra-test-login-type0@example.com",
                password = "Test1234!",
                timezone = "UTC",
                userType = 0  // UserType.Creator
            });
            var response2 = await _client.PostAsJsonAsync("/auth/register", new
            {
                name = "Test User",
                email = "infra-test-login-type1@example.com",
                password = "Test1234!",
                timezone = "UTC",
                userType = 1  // UserType.Member
            });
            var response3 = await _client.PostAsJsonAsync("/auth/register", new
            {
                name = "Test User",
                email = "infra-test-login-shared@example.com",
                password = "Test1234!",
                timezone = "UTC",
                userType = 0  // UserType.Creator
            });
            var response4 = await _client.PostAsJsonAsync("/auth/register", new
            {
                name = "Test User",
                email = "infra-test-login-shared@example.com",
                password = "Test1234!",
                timezone = "UTC",
                userType = 1  // UserType.Member
            });

            Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
            Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
            Assert.Equal(HttpStatusCode.Created, response3.StatusCode);
            Assert.Equal(HttpStatusCode.Created, response4.StatusCode);
        }
    }
    
    private readonly HttpClient _client;

    public AuthLoginEndpointsTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Login_WithValidData_Returns200AndSessionForCorrectUser(int userType)
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"infra-test-login-type{userType}@example.com",
            password = "Test1234!",
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.NotNull(body.User);
        Assert.Equal($"infra-test-login-type{userType}@example.com", body.User.Email);
        Assert.Equal(userType, (int)body.User.UserType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Login_WithEmailSharedBetweenTwoAccounts_Returns200AndSessionForCorrectUser(int userType)
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
        Assert.Equal(userType, (int)body.User.UserType);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public async Task Login_WithUserTypeMismatch_Returns401(int userTypeA, int userTypeB)
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
            //userType = 0  // UserType.Creator
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
            userType = 0  // UserType.Creator
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
