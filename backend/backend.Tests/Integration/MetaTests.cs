using backend.Tests.Fixtures;

namespace backend.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class MetaTests
{
    private readonly HttpClient _client;

    public MetaTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    // This test is ran twice, since running it once wouldn't achieve anything
    [Fact]
    public void EnsureThat_HttpClient_Context_DoesNotLeakBetweenTests_A()
    {
        Assert.False(_client.DefaultRequestHeaders.Contains("X-Session-Id"));
        _client.DefaultRequestHeaders.Add("X-Session-Id", Guid.NewGuid().ToString());
    }
    [Fact]
    public void EnsureThat_HttpClient_Context_DoesNotLeakBetweenTests_B()
    {
        Assert.False(_client.DefaultRequestHeaders.Contains("X-Session-Id"));
        _client.DefaultRequestHeaders.Add("X-Session-Id", Guid.NewGuid().ToString());
    }
}