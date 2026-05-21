using System.Net;
using System.Net.Http.Json;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitDeleteHabitEndpointTests
{
    private readonly HttpClient _client;

    public HabitDeleteHabitEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeleteHabit_WithoutSession_Returns401()
    {
        var response = await _client.DeleteAsync($"/habits/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteHabit_AsCreator_Returns204()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await _client.DeleteAsync($"/habits/{habit.HabitId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var get = await _client.GetAsync($"/habits/{habit.HabitId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task DeleteHabit_AsNonOwner_Returns403()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);

        var response = await _client.DeleteAsync($"/habits/{habit.HabitId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteHabit_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.DeleteAsync($"/habits/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteHabit_CascadesEntriesAndReminders()
    {
        var (creatorSession, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var (memberSession, _, _) = await TestUtils.SetupMember(_client, code);
        await TestUtils.HabitLog(_client, habit.HabitId, EntryStatus.Logged);

        // Member opts into reminders to create a reminder row.
        var reminderResp = await _client.PatchAsJsonAsync($"/habits/{habit.HabitId}/my-reminder", new { enabled = true }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, reminderResp.StatusCode);

        // Creator deletes the habit.
        TestUtils.SetSession(_client, creatorSession);
        var delete = await _client.DeleteAsync($"/habits/{habit.HabitId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        // Habit is gone — anything referencing it must 404 (entries and reminders are cascaded).
        TestUtils.SetSession(_client, memberSession);
        var entries = await _client.GetAsync($"/habits/{habit.HabitId}/entries", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, entries.StatusCode);

        var myReminder = await _client.GetAsync($"/habits/{habit.HabitId}/my-reminder", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, myReminder.StatusCode);
    }
}
