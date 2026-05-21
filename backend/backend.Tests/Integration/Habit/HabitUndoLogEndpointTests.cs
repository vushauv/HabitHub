using System.Net;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitUndoLogEndpointTests
{
    private readonly HttpClient _client;

    public HabitUndoLogEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UndoLog_WithoutSession_Returns401()
    {
        var response = await _client.DeleteAsync($"/habits/{Guid.NewGuid()}/entries/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UndoLog_AsMember_Returns204()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);
        var entry = await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Logged);

        var response = await _client.DeleteAsync($"/habits/{habit.HabitId}/entries/{entry.EntryId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UndoLog_AsCreator_Returns403()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await _client.DeleteAsync($"/habits/{habit.HabitId}/entries/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UndoLog_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Member);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.DeleteAsync($"/habits/{Guid.NewGuid()}/entries/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UndoLog_NoEntryToday_Returns404()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);

        var response = await _client.DeleteAsync($"/habits/{habit.HabitId}/entries/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UndoLog_WrongEntryId_Returns404()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);
        await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Logged);

        var response = await _client.DeleteAsync($"/habits/{habit.HabitId}/entries/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UndoLog_HabitArchived_Returns409()
    {
        var (creatorSession, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var (memberSession, _, _) = await TestUtils.SetupMember(_client, code);
        var entry = await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Logged);

        TestUtils.SetSession(_client, creatorSession);
        await _client.PostAsync($"/habits/{habit.HabitId}/archive", null, TestContext.Current.CancellationToken);

        TestUtils.SetSession(_client, memberSession);
        var response = await _client.DeleteAsync($"/habits/{habit.HabitId}/entries/{entry.EntryId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
