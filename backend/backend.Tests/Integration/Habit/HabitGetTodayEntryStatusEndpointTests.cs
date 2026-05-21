using System.Net;
using System.Net.Http.Json;
using backend.Dtos.HabitEntryDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitGetTodayEntryStatusEndpointTests
{
    private readonly HttpClient _client;

    public HabitGetTodayEntryStatusEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetToday_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync($"/habits/{Guid.NewGuid()}/entries/today", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetToday_NoEntry_ReturnsPending()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/entries/today", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodayHabitEntryStatusDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(EntryStatus.Pending, body.Status);
        Assert.Null(body.Entry);
    }

    [Fact]
    public async Task GetToday_AfterLogged_ReturnsLogged()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);
        await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Logged);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/entries/today", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodayHabitEntryStatusDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(EntryStatus.Logged, body.Status);
        Assert.NotNull(body.Entry);
    }

    [Fact]
    public async Task GetToday_AfterSkipped_ReturnsSkipped()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);
        await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Skipped);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/entries/today", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodayHabitEntryStatusDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(EntryStatus.Skipped, body.Status);
        Assert.NotNull(body.Entry);
    }
}
