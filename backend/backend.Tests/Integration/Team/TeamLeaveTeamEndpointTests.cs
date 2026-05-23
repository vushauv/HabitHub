using System.Net;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Team;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class TeamLeaveTeamEndpointTests
{
    private readonly HttpClient _client;

    public TeamLeaveTeamEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LeaveTeam_WithoutSession_Returns401()
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
        
        var response = await _client.PostAsync($"/teams/{teamId}/leave", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LeaveTeam_WithTeamMemberSession_Returns200_AndTryingToLeaveAgain_Returns404()
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
        
        var response = await _client.PostAsync($"/teams/{teamId}/leave", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var response2 = await _client.PostAsync($"/teams/{teamId}/leave", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
    }

    [Fact]
    public async Task LeaveTeam_WithTeamCreatorSession_Returns403()
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
        
        var response = await _client.PostAsync($"/teams/{teamId}/leave", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
