using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task<TeamChat?> GetChatByTeamIdAsync(Guid teamId);
        Task<TeamChat> CreateChatAsync(TeamChat chat);
        Task<List<Message>> GetMessageByTeamIdAsync(Guid teamId);
        Task<Message?> GetMessageByIdAsync(Guid messageId);
        Task<Message?> GetMessageByIdAndTeamIdAsync(Guid messageId, Guid teamId);
        Task <Message> CreateMessageAsync(Message message);
        Task<bool> DeleteMessageAsync(Guid messageId);
    }
}
