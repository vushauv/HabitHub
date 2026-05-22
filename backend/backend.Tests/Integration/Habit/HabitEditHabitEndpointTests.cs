using System.Net;
using System.Net.Http.Json;
using backend.Dtos.HabitDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitEditHabitEndpointTests
{
    private readonly HttpClient _client;

    public HabitEditHabitEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static async Task<HttpResponseMessage> Patch(HttpClient client, Guid habitId, object body) =>
        await client.PatchAsJsonAsync($"/habits/{habitId}", body, TestContext.Current.CancellationToken);

    [Fact]
    public async Task EditHabit_WithoutSession_Returns401()
    {
        var response = await Patch(_client, Guid.NewGuid(), new { name = "x" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EditHabit_AsCreator_Returns200()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await Patch(_client, habit.HabitId, new { name = "Renamed", goal = "New goal" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HabitSummaryDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal("Renamed", body.Name);
        Assert.Equal("New goal", body.Goal);
    }

    [Fact]
    public async Task EditHabit_EmptyName_Returns400()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await Patch(_client, habit.HabitId, new { name = "   " });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditHabit_PastExpiryDate_Returns400()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await Patch(_client, habit.HabitId, new { expiryDate = DateTime.UtcNow.AddDays(-1) });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditHabit_ClearGoalAndDefineGoal_Returns400()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await Patch(_client, habit.HabitId, new { goal = "g", clearGoal = true });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditHabit_ClearExpiryAndDefineExpiry_Returns400()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await Patch(_client, habit.HabitId, new
        {
            expiryDate = DateTime.UtcNow.AddDays(7),
            clearExpiryDate = true
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditHabit_AsNonOwner_Returns403()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        await TestUtils.SetupMember(_client, code);

        var response = await Patch(_client, habit.HabitId, new { name = "x" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EditHabit_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, sessionId);

        var response = await Patch(_client, Guid.NewGuid(), new { name = "x" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EditHabit_Archived_Returns409()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var archive = await _client.PostAsync($"/habits/{habit.HabitId}/archive", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);

        var response = await Patch(_client, habit.HabitId, new { name = "x" });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
