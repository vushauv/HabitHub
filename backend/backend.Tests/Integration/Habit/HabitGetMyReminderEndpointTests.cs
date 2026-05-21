using System.Net;
using System.Net.Http.Json;
using backend.Dtos.ReminderDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitGetMyReminderEndpointTests
{
    private readonly HttpClient _client;

    public HabitGetMyReminderEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMyReminder_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync($"/habits/{Guid.NewGuid()}/my-reminder", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyReminder_AsMember_Returns200()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var (_, memberId, _) = await TestUtils.SetupMember(_client, code);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/my-reminder", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<MyReminderResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(memberId, body.MemberId);
        Assert.Equal(habit.HabitId, body.HabitId);
    }

    [Fact]
    public async Task GetMyReminder_AsCreator_Returns403()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await _client.GetAsync($"/habits/{habit.HabitId}/my-reminder", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMyReminder_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Member);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.GetAsync($"/habits/{Guid.NewGuid()}/my-reminder", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
