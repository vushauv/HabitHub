using System.Net;
using System.Net.Http.Json;
using backend.Dtos.HabitDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitGetTeamHabitsEndpointTests
{
    private readonly HttpClient _client;

    public HabitGetTeamHabitsEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTeamHabits_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync($"/teams/{Guid.NewGuid()}/habits?state=Active", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTeamHabits_AsCreator_Returns200()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        await TestUtils.HabitCreate(_client, teamId);

        var response = await _client.GetAsync($"/teams/{teamId}/habits?state=Active", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<HabitSummaryDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
    }

    [Fact]
    public async Task GetTeamHabits_AsActiveMember_Returns200()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);

        var response = await _client.GetAsync($"/teams/{teamId}/habits?state=Active", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<HabitSummaryDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
    }

    [Fact]
    public async Task GetTeamHabits_InvalidState_Returns400()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);

        var response = await _client.GetAsync($"/teams/{teamId}/habits?state=Closed", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTeamHabits_AsNonMemberCreator_Returns403()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);

        var outsider = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, outsider);

        var response = await _client.GetAsync($"/teams/{teamId}/habits?state=Active", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetTeamHabits_TeamMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.GetAsync($"/teams/{Guid.NewGuid()}/habits?state=Active", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
