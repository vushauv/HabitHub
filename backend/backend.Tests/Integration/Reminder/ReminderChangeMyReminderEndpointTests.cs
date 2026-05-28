using System.Net;
using System.Net.Http.Json;
using backend.Dtos.ReminderDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitChangeMyReminderEndpointTests
{
    private readonly HttpClient _client;

    public HabitChangeMyReminderEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<HttpResponseMessage> Change(Guid habitId, bool enabled) =>
        await _client.PatchAsJsonAsync($"/habits/{habitId}/my-reminder", new { enabled }, TestContext.Current.CancellationToken);

    [Fact]
    public async Task ChangeMyReminder_WithoutSession_Returns401()
    {
        var response = await Change(Guid.NewGuid(), true);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeMyReminder_AsMember_Returns200()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var (_, memberId, _) = await TestUtils.SetupMember(_client, code);

        var response = await Change(habit.HabitId, false);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<MyReminderResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(memberId, body.MemberId);
        Assert.False(body.Enabled);
    }

    [Fact]
    public async Task ChangeMyReminder_AsCreator_Returns403()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await Change(habit.HabitId, true);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ChangeMyReminder_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Member);
        TestUtils.SetSession(_client, sessionId);

        var response = await Change(Guid.NewGuid(), true);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
