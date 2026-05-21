using System.Net;
using System.Net.Http.Json;
using backend.Dtos.HabitEntryDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitViewProgressEndpointTests
{
    private readonly HttpClient _client;

    public HabitViewProgressEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ViewProgress_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync($"/habits/{Guid.NewGuid()}/entries", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ViewProgress_Self_Returns200()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);
        await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Logged);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/entries", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<HabitEntryResponseDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
    }

    [Fact]
    public async Task ViewProgress_OtherMember_Returns200()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var (_, otherMemberId, _) = await TestUtils.SetupMember(_client, code);
        await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Logged);

        await TestUtils.SetupMember(_client, code);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/entries?memberId={otherMemberId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<HabitEntryResponseDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
    }

    [Fact]
    public async Task ViewProgress_AsCreator_Returns403()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/entries", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ViewProgress_TargetNotActive_Returns404()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/entries?memberId={Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
