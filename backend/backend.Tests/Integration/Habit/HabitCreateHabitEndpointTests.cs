using System.Net;
using System.Net.Http.Json;
using backend.Dtos.HabitDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Habit;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class HabitCreateHabitEndpointTests
{
    private readonly HttpClient _client;

    public HabitCreateHabitEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateHabit_WithoutSession_Returns401()
    {
        var response = await _client.PostAsJsonAsync($"/teams/{Guid.NewGuid()}/habits", new
        {
            name = "Habit"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateHabit_AsCreator_WithValidRequest_Returns201()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);

        var response = await _client.PostAsJsonAsync($"/teams/{teamId}/habits", new
        {
            name = "Run",
            habitType = "Quantitative",
            unit = "Km"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateHabitResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(teamId, body.TeamId);
        Assert.Equal("Run", body.Name);
        Assert.Equal(HabitType.Quantitative, body.HabitType);
        Assert.Equal(backend.Enums.Unit.Km, body.Unit);
        Assert.Equal(HabitState.Active, body.HabitState);
    }

    [Fact]
    public async Task CreateHabit_BinaryHabitWithUnit_Returns400()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);

        var response = await _client.PostAsJsonAsync($"/teams/{teamId}/habits", new
        {
            name = "Read",
            habitType = "Binary",
            unit = "Pages"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateHabit_QuantitativeWithoutUnit_Returns400()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);

        var response = await _client.PostAsJsonAsync($"/teams/{teamId}/habits", new
        {
            name = "Run",
            habitType = "Quantitative"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateHabit_PastExpiryDate_Returns400()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);

        var response = await _client.PostAsJsonAsync($"/teams/{teamId}/habits", new
        {
            name = "Run",
            expiryDate = DateTime.UtcNow.AddDays(-1)
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateHabit_AsNonOwner_Returns403()
    {
        var (_, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);
        await TestUtils.SetupMember(_client, code);

        var response = await _client.PostAsJsonAsync($"/teams/{teamId}/habits", new
        {
            name = "Habit"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateHabit_TeamMissing_Returns404()
    {
        var sessionId = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.PostAsJsonAsync($"/teams/{Guid.NewGuid()}/habits", new
        {
            name = "Habit"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
