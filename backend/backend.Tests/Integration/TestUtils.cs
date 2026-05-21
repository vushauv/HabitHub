using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
using backend.Dtos.HabitDtos;
using backend.Dtos.HabitEntryDtos;
using backend.Dtos.TeamDtos;
using backend.Enums;

namespace backend.Tests.Integration;

public static class TestUtils
{
    public static async Task<string> AuthRegister(HttpClient client, string name, string email, string password, string timezone, UserType userType)
    {
        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            name,
            email,
            password,
            timezone,
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body.SessionId;
    }
    public static async Task<string> AuthLogIn(HttpClient client, string email, string password, UserType userType)
    {
        var response = await client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password,
            userType
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body.SessionId;
    }

    public static async Task<Guid> TeamCreate(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/teams", new
        {
            name
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateTeamResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body.TeamId;
    }

    public static async Task<string> TeamGenerateInviteCode(HttpClient client, Guid teamId)
    {
        var response = await client.PostAsync($"/teams/{teamId}/invite-codes", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InviteCodeDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body.Code;
    }

    public static async Task<(Guid teamId, Guid memberId)> TeamJoin(HttpClient client, string code)
    {
        var response = await client.PostAsJsonAsync("/teams/join", new
        {
            code
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JoinTeamResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return (body.TeamId, body.MemberId);
    }
    
    public static void SetSession(HttpClient client, string sessionId)
    {
        client.DefaultRequestHeaders.Remove("X-Session-Id");
        client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
    }
    

    public static async Task<string> RegisterFresh(HttpClient client, UserType userType)
    {
        var uuid = Guid.NewGuid().ToString();
        return await AuthRegister(client, uuid, $"{uuid}@example.com", "Test1234!", "UTC", userType);
    }

    // Registers a new Creator, sets session, creates team. Returns (sessionId, teamId).
    public static async Task<(string sessionId, Guid teamId)> SetupCreatorWithTeam(HttpClient client)
    {
        var sessionId = await RegisterFresh(client, UserType.Creator);
        SetSession(client, sessionId);
        var teamId = await TeamCreate(client, Guid.NewGuid().ToString());
        return (sessionId, teamId);
    }

    // Registers a new Creator, sets session, creates team and generates invite code.
    public static async Task<(string sessionId, Guid teamId, string code)> SetupCreatorTeamWithCode(HttpClient client)
    {
        var (sessionId, teamId) = await SetupCreatorWithTeam(client);
        var code = await TeamGenerateInviteCode(client, teamId);
        return (sessionId, teamId, code);
    }

    // Registers a new Member, sets session, joins team via code. Returns (sessionId, memberId, teamId).
    public static async Task<(string sessionId, Guid memberId, Guid teamId)> SetupMember(HttpClient client, string code)
    {
        var sessionId = await RegisterFresh(client, UserType.Member);
        SetSession(client, sessionId);
        var (teamId, memberId) = await TeamJoin(client, code);
        return (sessionId, memberId, teamId);
    }

    public static async Task<CreateHabitResponseDto> HabitCreate(
        HttpClient client,
        Guid teamId,
        HabitType habitType = HabitType.Binary,
        backend.Enums.Unit? unit = null,
        string? name = null,
        string? goal = null,
        DateTime? expiryDate = null)
    {
        var response = await client.PostAsJsonAsync($"/teams/{teamId}/habits", new
        {
            name = name ?? Guid.NewGuid().ToString(),
            goal,
            habitType = habitType.ToString(),
            unit = unit?.ToString(),
            expiryDate
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateHabitResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body;
    }

    public static async Task<HabitEntryResponseDto> HabitLog(
        HttpClient client,
        Guid habitId,
        EntryStatus status = EntryStatus.Logged,
        float? value = null,
        string? notes = null)
    {
        var response = await client.PostAsJsonAsync($"/habits/{habitId}/entries", new
        {
            status = status.ToString(),
            value,
            notes
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HabitEntryResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body;
    }
}
