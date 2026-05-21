using System.Net;
using System.Net.Http.Json;
using backend.Dtos.TeamDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Team;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class TeamJoinTeamEndpointTests
{
    private readonly HttpClient _client;

    public TeamJoinTeamEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task JoinTeam_WithoutSession_Returns401()
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
        
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        
        var response = await _client.PostAsJsonAsync("/teams/join", new
        {
            code
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JoinTeam_WithTeamMemberSession_Returns200_AndTryingToJoinAgain_Returns409()
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
        
        // Join team
        var response = await _client.PostAsJsonAsync("/teams/join", new
        {
            code
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JoinTeamResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(teamId, body.TeamId);
        
        // Try to join again
        var response2 = await _client.PostAsJsonAsync("/teams/join", new
        {
            code
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }

    [Fact]
    public async Task JoinTeam_WithTeamCreatorSession_Returns403()
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
            0);
        
        // Set session header
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        _client.DefaultRequestHeaders.Add("X-Session-Id", sessionId2);
        
        var response = await _client.PostAsJsonAsync("/teams/join", new
        {
            code
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
