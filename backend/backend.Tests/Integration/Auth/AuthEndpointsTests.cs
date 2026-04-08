using System.Net;
using System.Net.Http.Json;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
public class AuthEndpointsTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Returns200()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
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
        });
        var response2 = await _client.PostAsJsonAsync("/auth/register", new
        {
            name = "Test User",
            email = "infra-test-2types@example.com",
            password = "Test1234!",
            timezone = "UTC",
            userType = 1  // UserType.Member
        });

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
    }
}
