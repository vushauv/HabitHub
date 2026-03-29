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
            userType = "TeamCreator"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
