using System.Net;
using System.Net.Http.Json;
using backend.Dtos.ReminderDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitArchiveHabitEndpointTests
{
    private readonly HttpClient _client;

    public HabitArchiveHabitEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ArchiveHabit_WithoutSession_Returns401()
    {
        var response = await _client.PostAsync($"/habits/{Guid.NewGuid()}/archive", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveHabit_AsCreator_Returns200()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await _client.PostAsync($"/habits/{habit.HabitId}/archive", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveHabit_Twice_IsIdempotent()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var first = await _client.PostAsync($"/habits/{habit.HabitId}/archive", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await _client.PostAsync($"/habits/{habit.HabitId}/archive", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    [Fact]
    public async Task ArchiveHabit_AsNonOwner_Returns403()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);

        var response = await _client.PostAsync($"/habits/{habit.HabitId}/archive", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveHabit_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.PostAsync($"/habits/{Guid.NewGuid()}/archive", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveHabit_DisablesMemberReminders()
    {
        var (creatorSession, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        // Creator sets reminder time so reminders are provisioned for members.
        var setReminder = await _client.PatchAsJsonAsync($"/habits/{habit.HabitId}/reminder", new
        {
            reminderTime = "09:00:00"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, setReminder.StatusCode);

        // Member joins after reminder is set — so creator re-sets to ensure provisioning includes them.
        var (memberSession, _, _) = await TestUtils.SetupMember(_client, code);

        TestUtils.SetSession(_client, creatorSession);
        await _client.PatchAsJsonAsync($"/habits/{habit.HabitId}/reminder", new { reminderTime = "09:00:00" }, TestContext.Current.CancellationToken);

        // Archive disables all reminders for habit.
        var archive = await _client.PostAsync($"/habits/{habit.HabitId}/archive", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);

        TestUtils.SetSession(_client, memberSession);
        var myReminder = await _client.GetAsync($"/habits/{habit.HabitId}/my-reminder", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, myReminder.StatusCode);

        var body = await myReminder.Content.ReadFromJsonAsync<MyReminderResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.False(body.Enabled);
    }
}
