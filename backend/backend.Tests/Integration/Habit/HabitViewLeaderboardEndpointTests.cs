using System.Net;
using System.Net.Http.Json;
using backend.Dtos.HabitEntryDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitViewLeaderboardEndpointTests
{
    private readonly HttpClient _client;

    public HabitViewLeaderboardEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Leaderboard_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync($"/habits/{Guid.NewGuid()}/leaderboard", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Leaderboard_Binary_OrdersByLoggedCount_AndAssignsRanks()
    {
        var (creatorSession, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var (_, memberA, _) = await TestUtils.SetupMember(_client, code);
        await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Logged);

        var (_, memberB, _) = await TestUtils.SetupMember(_client, code);
        await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Skipped);

        TestUtils.SetSession(_client, creatorSession);
        var response = await _client.GetAsync($"/habits/{habit.HabitId}/leaderboard", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<LeaderboardResponseDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.Equal(memberA, body[0].MemberId);
        Assert.Equal(1, body[0].Rank);
        Assert.Equal(memberB, body[1].MemberId);
        Assert.Equal(2, body[1].Rank);
    }

    [Fact]
    public async Task Leaderboard_Quantitative_OrdersByTotalValue()
    {
        var (creatorSession, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId, HabitType.Quantitative, backend.Enums.Unit.Km);

        var (_, memberA, _) = await TestUtils.SetupMember(_client, code);
        await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Logged, value: 2.0f);

        var (_, memberB, _) = await TestUtils.SetupMember(_client, code);
        await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Logged, value: 10.0f);

        TestUtils.SetSession(_client, creatorSession);
        var response = await _client.GetAsync($"/habits/{habit.HabitId}/leaderboard", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<LeaderboardResponseDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.Equal(memberB, body[0].MemberId);
        Assert.Equal(1, body[0].Rank);
        Assert.Equal(memberA, body[1].MemberId);
        Assert.Equal(2, body[1].Rank);
    }

    [Fact]
    public async Task Leaderboard_NoEntries_ReturnsEmpty()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/leaderboard", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<LeaderboardResponseDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task Leaderboard_AsOutsider_Returns403()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var outsider = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, outsider);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/leaderboard", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Leaderboard_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.GetAsync($"/habits/{Guid.NewGuid()}/leaderboard", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
