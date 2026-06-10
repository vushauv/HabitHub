using backend.Auth;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs;

[Authorize]
public class ChatHub(IChatService chatService) : Hub<IChatClient>
{
    public async Task JoinTeam(Guid teamId)
    {
        var currentUser = Context.GetHttpContext()!.RequireCurrentUser();
        try
        {
            await chatService.GetMessages(currentUser.UserId, currentUser.UserType, teamId, 0, 1);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"team-{teamId}");
        }
        catch
        {
            // user has no access to this team — don't add to group
        }
    }
}
