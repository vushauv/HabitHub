using System.Net;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Team;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class TeamKickUserEndpointTests
{
    private readonly HttpClient _client;

    public TeamKickUserEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task KickUser_WithoutSession_Returns401()
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
        
        var response = await _client.PostAsync($"/teams/{teamId}/members/{uuid}/kick", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task KickUser_WithTeamMemberSession_Returns403()
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
            1);
        var sessionId3 = await TestUtils.AuthRegister(
            _client,
            uuid,
            $"{uuid}3@example.com",
            "Test1234!",
            "UTC",
            1);
        
        // Set session header
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId2);
        
        var (_,memberId) = await TestUtils.TeamJoin(_client, code);
        
        // Set session header
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId3);
        
        await TestUtils.TeamJoin(_client, code);
        
        var response = await _client.PostAsync($"/teams/{teamId}/members/{memberId}/kick", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task KickTeam_WithTeamCreatorSession_Returns200_AndTryingToKickAgain_Returns404()
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
            1);
        
        // Set session header
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId2);
        
        var (_,memberId) = await TestUtils.TeamJoin(_client, code);
        
        // Set session header
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        
        var response = await _client.PostAsync($"/teams/{teamId}/members/{memberId}/kick", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var response2 = await _client.PostAsync($"/teams/{teamId}/members/{memberId}/kick", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
    }
}
