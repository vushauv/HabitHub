using System.Net;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Team;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class TeamDeleteTeamEndpointTests
{
    private readonly HttpClient _client;

    public TeamDeleteTeamEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeleteTeam_WithoutSession_Returns401()
    {
        var uuid = Guid.NewGuid().ToString();
        
        // Register user
        var sessionId = await TestUtils.AuthRegister(
            _client,
            uuid,
            $"{uuid}@example.com",
            "Test1234!",
            "UTC",
            0);
        
        // Set session header
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        // Create Team
        var teamId = await TestUtils.TeamCreate(_client, uuid);
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response = await _client.DeleteAsync($"/teams/{teamId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTeam_WithTeamMemberSession_Returns403()
    {
        var uuid = Guid.NewGuid().ToString();
        
        // Register user
        var sessionId = await TestUtils.AuthRegister(
            _client,
            uuid,
            $"{uuid}@example.com",
            "Test1234!",
            "UTC",
            0);
        
        // Set session header
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        // Create Team
        var teamId = await TestUtils.TeamCreate(_client, uuid);
        var code = await TestUtils.TeamGenerateInviteCode(_client, teamId);
        
        // Register user
        var sessionId2 = await TestUtils.AuthRegister(
            _client,
            uuid,
            $"{uuid}2@example.com",
            "Test1234!",
            "UTC",
            UserType.Member);
        
        // Set session header
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId2);
        
        await TestUtils.TeamJoin(_client, code);
        
        var response = await _client.DeleteAsync($"/teams/{teamId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTeam_WithTeamCreatorSession_Returns204_AndTryingToDeleteAgain_Returns404()
    {
        var uuid = Guid.NewGuid().ToString();
        
        // Register user
        var sessionId = await TestUtils.AuthRegister(
            _client,
            uuid,
            $"{uuid}@example.com",
            "Test1234!",
            "UTC",
            0);
        
        // Set session header
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        // Create Team
        var teamId = await TestUtils.TeamCreate(_client, uuid);
        
        var response = await _client.DeleteAsync($"/teams/{teamId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var response2 = await _client.DeleteAsync($"/teams/{teamId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
    }
}
