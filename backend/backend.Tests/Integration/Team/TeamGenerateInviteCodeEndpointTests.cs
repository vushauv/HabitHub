using System.Net;
using System.Net.Http.Json;
using backend.Dtos.TeamDtos;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Team;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class TeamGenerateInviteCodeEndpointTests
{
    private readonly HttpClient _client;

    public TeamGenerateInviteCodeEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateTeamInviteCode_WithoutSession_Returns401()
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
        
        var response = await _client.PostAsync($"/teams/{teamId}/invite-codes", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GenerateTeamInviteCode_WithSession_Returns201_AndCode()
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
        
        var response = await _client.PostAsync($"/teams/{teamId}/invite-codes", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InviteCodeDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(teamId, body.TeamId);
        Assert.NotEmpty(body.Code);
    }

    [Fact]
    public async Task GenerateTeamInviteCode_WithSession_NotAsMemberOrCreatorOfTeam_Returns403()
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
        
        var response = await _client.PostAsync($"/teams/{teamId}/invite-codes", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
