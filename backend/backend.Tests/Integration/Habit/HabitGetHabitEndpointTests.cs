using System.Net;
using System.Net.Http.Json;
using backend.Dtos.HabitDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitGetHabitEndpointTests
{
    private readonly HttpClient _client;

    public HabitGetHabitEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHabit_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync($"/habits/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetHabit_AsCreator_Returns200()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HabitSummaryDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(habit.HabitId, body.HabitId);
    }

    [Fact]
    public async Task GetHabit_AsActiveMember_Returns200()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);
        await TestUtils.SetupMember(_client, code);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHabit_AsOutsider_Returns403()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var outsider = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, outsider);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetHabit_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.GetAsync($"/habits/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
