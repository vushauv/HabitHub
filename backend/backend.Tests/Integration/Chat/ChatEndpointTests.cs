using System.Net;
using System.Net.Http.Json;
using backend.Dtos.ChatDtos;
using backend.Enums;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.Chat;

[Trait("Category", "Integration")]
[Collection("Web app collection")]
public class ChatEndpointTests
{
    private readonly HttpClient _client;

    public ChatEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMessages_WithoutSession_Returns401()
    {
        var response = await _client.GetAsync(
            $"/teams/{Guid.NewGuid()}/chat/messages",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_AsCreator_Returns200()
    {
        var (sessionId, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.GetAsync(
            $"/teams/{teamId}/chat/messages",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<MessageDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetMessages_AsNonMember_Returns403()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);

        var outsiderSession = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, outsiderSession);

        var response = await _client.GetAsync(
            $"/teams/{teamId}/chat/messages",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_AsCreator_Returns201WithBody()
    {
        var (sessionId, teamId) = await TestUtils.SetupCreatorWithTeam(_client);
        TestUtils.SetSession(_client, sessionId);

        var response = await _client.PostAsJsonAsync(
            $"/teams/{teamId}/chat/messages",
            new { content = "hello world" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MessageDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal("hello world", body.Content);
    }

    [Fact]
    public async Task SendMessage_AsNonMember_Returns403()
    {
        var (_, teamId) = await TestUtils.SetupCreatorWithTeam(_client);

        var outsiderSession = await TestUtils.RegisterFresh(_client, UserType.Creator);
        TestUtils.SetSession(_client, outsiderSession);

        var response = await _client.PostAsJsonAsync(
            $"/teams/{teamId}/chat/messages",
            new { content = "hello" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMessage_AsNonAuthor_Returns403()
    {
        var (creatorSession, teamId, code) = await TestUtils.SetupCreatorTeamWithCode(_client);

        var (memberSession, _, _) = await TestUtils.SetupMember(_client, code);

        TestUtils.SetSession(_client, creatorSession);
        var sent = await TestUtils.ChatSendMessage(_client, teamId, "to be deleted");

        TestUtils.SetSession(_client, memberSession);
        var response = await _client.DeleteAsync(
            $"/teams/{teamId}/chat/messages/{sent.MessageId}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
