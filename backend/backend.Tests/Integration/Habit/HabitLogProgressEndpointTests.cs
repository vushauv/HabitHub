using System.Net;
using System.Net.Http.Json;
using backend.Dtos.HabitEntryDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitLogProgressEndpointTests
{
    private readonly HttpClient _client;

    public HabitLogProgressEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(string creatorSession, Guid habitId, string memberSession)> SetupBinaryHabitWithMember()
    {
        var (creatorSession, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);
        var (memberSession, _, _) = await TestUtils.SetupMember(_client, code);
        return (creatorSession, habit.HabitId, memberSession);
    }

    private async Task<(string creatorSession, Guid habitId, string memberSession)> SetupQuantitativeHabitWithMember()
    {
        var (creatorSession, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId, HabitType.Quantitative, backend.Enums.Unit.Km);
        var (memberSession, _, _) = await TestUtils.SetupMember(_client, code);
        return (creatorSession, habit.HabitId, memberSession);
    }

    private async Task<HttpResponseMessage> Log(Guid habitId, object body) =>
        await _client.PostAsJsonAsync($"/habits/{habitId}/entries", body, TestContext.Current.CancellationToken);

    [Fact]
    public async Task LogProgress_WithoutSession_Returns401()
    {
        var response = await Log(Guid.NewGuid(), new { status = "Logged" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LogProgress_Binary_Returns201()
    {
        var (_, habitId, _) = await SetupBinaryHabitWithMember();

        var response = await Log(habitId, new { status = "Logged" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HabitEntryResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(EntryStatus.Logged, body.Status);
        Assert.Null(body.Value);
    }

    [Fact]
    public async Task LogProgress_Quantitative_Returns201()
    {
        var (_, habitId, _) = await SetupQuantitativeHabitWithMember();

        var response = await Log(habitId, new { status = "Logged", value = 5.5 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HabitEntryResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(5.5f, body.Value);
    }

    [Fact]
    public async Task LogProgress_PendingStatus_Returns400()
    {
        var (_, habitId, _) = await SetupBinaryHabitWithMember();

        var response = await Log(habitId, new { status = "Pending" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LogProgress_SkippedWithValue_Returns400()
    {
        var (_, habitId, _) = await SetupQuantitativeHabitWithMember();

        var response = await Log(habitId, new { status = "Skipped", value = 1.0 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LogProgress_BinaryWithValue_Returns400()
    {
        var (_, habitId, _) = await SetupBinaryHabitWithMember();

        var response = await Log(habitId, new { status = "Logged", value = 1.0 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LogProgress_QuantitativeWithoutValue_Returns400()
    {
        var (_, habitId, _) = await SetupQuantitativeHabitWithMember();

        var response = await Log(habitId, new { status = "Logged" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LogProgress_AsCreator_Returns403()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        var habit = await TestUtils.HabitCreate(_client, teamId);

        var response = await Log(habit.HabitId, new { status = "Logged" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task LogProgress_HabitMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Member);
        TestUtils.SetSession(_client, sessionId);

        var response = await Log(Guid.NewGuid(), new { status = "Logged" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LogProgress_HabitArchived_Returns409()
    {
        var (creatorSession, habitId, memberSession) = await SetupBinaryHabitWithMember();

        TestUtils.SetSession(_client, creatorSession);
        await _client.PostAsync($"/habits/{habitId}/archive", null, TestContext.Current.CancellationToken);

        TestUtils.SetSession(_client, memberSession);
        var response = await Log(habitId, new { status = "Logged" });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task LogProgress_DuplicateToday_Returns409()
    {
        var (_, habitId, _) = await SetupBinaryHabitWithMember();

        var first = await Log(habitId, new { status = "Logged" });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await Log(habitId, new { status = "Logged" });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }
}
