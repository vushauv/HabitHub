using backend.Dtos.ChatDtos;
using backend.Enums;

namespace backend.Service.Interfaces
{
    public interface IChatService
    {
        public Task<List<MessageDto>> GetMessages(Guid userId, UserType userType, Guid teamId, int offset, int count);
        public Task<MessageDto> SendMessage(Guid userId, UserType userType, Guid teamId, SendMessageRequestDto request);
        public Task DeleteMessage(Guid userId, UserType userType, Guid teamId, Guid messageId);
    }
}
