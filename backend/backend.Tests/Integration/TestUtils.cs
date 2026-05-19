using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;
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
}