using backend.Dtos.ChatDtos;

namespace backend.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(MessageDto message);
    Task MessageDeleted(Guid messageId);
}
