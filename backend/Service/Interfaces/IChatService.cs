using backend.Dtos.ChatDtos;
using backend.Models;

namespace backend.Service.Interfaces
{
    public interface IChatService
    {
        public Task<List<MessageDto>> GetMessages(User user, Guid teamId, int offset, int count);
        public Task<MessageDto> SendMessage(User user, Guid teamId, SendMessageRequestDto request);
        public Task DeleteMessage(User user, Guid teamId, Guid messageId);
    }
}
