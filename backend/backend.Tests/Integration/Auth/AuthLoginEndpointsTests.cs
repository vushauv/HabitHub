using System.Net;
using System.Net.Http.Json;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Auth;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class AuthLoginEndpointsTests
{
    private readonly HttpClient _client;

    public AuthLoginEndpointsTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }
}
