using System.Net;
using System.Net.Http.Json;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Reminder;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class ReminderClearReminderEndpointTests
{
    private readonly HttpClient _client;

    public ReminderClearReminderEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ClearReminder_WithoutSession_Returns401()
    {
        var response = await _client.DeleteAsync($"/habits/{Guid.NewGuid()}/reminder", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ClearReminder_AsCreator_Returns204()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await _client.PatchAsJsonAsync($"/habits/{habit.HabitId}/reminder", new { reminderTime = "09:00:00" }, TestContext.Current.CancellationToken);

        var response = await _client.DeleteAsync($"/habits/{habit.HabitId}/reminder", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ClearReminder_AsMember_Returns403()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);

        var response = await _client.DeleteAsync($"/habits/{habit.HabitId}/reminder", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ClearReminder_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.DeleteAsync($"/habits/{Guid.NewGuid()}/reminder", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
