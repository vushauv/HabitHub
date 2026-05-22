using System.Net;
using System.Net.Http.Json;
using backend.Dtos.ReminderDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitSetReminderEndpointTests
{
    private readonly HttpClient _client;

    public HabitSetReminderEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<HttpResponseMessage> SetReminder(Guid habitId, string time) =>
        await _client.PatchAsJsonAsync($"/habits/{habitId}/reminder", new { reminderTime = time }, TestContext.Current.CancellationToken);

    [Fact]
    public async Task SetReminder_WithoutSession_Returns401()
    {
        var response = await SetReminder(Guid.NewGuid(), "09:00:00");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SetReminder_AsCreator_Returns200()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await SetReminder(habit.HabitId, "09:00:00");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HabitReminderResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(habit.HabitId, body.HabitId);
        Assert.Equal(new TimeOnly(9, 0), body.ReminderTime);
    }

    [Fact]
    public async Task SetReminder_AsMember_Returns403()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);

        var response = await SetReminder(habit.HabitId, "09:00:00");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SetReminder_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, sessionId);

        var response = await SetReminder(Guid.NewGuid(), "09:00:00");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
