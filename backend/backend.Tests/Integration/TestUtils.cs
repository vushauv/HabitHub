using System.Net;
using System.Net.Http.Json;
using backend.Dtos.AuthDtos;

namespace backend.Tests.Integration;

public static class TestUtils
{
    public static async Task<string> AuthRegister(HttpClient client, string name, string email, string password, string timezone, int userType)
    {
        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            name,
            email,
            password,
            timezone,
            userType
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body.SessionId;
    }
    public static async Task<string> AuthLogIn(HttpClient client, string email, string password, int userType)
    {
        var response = await client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password,
            userType
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        return body.SessionId;
    }
}