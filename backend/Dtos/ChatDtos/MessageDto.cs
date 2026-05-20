using backend.Enums;

namespace backend.Dtos.ChatDtos
{
    public record MessageDto(
        Guid MessageId,
        Guid UserId,
        UserType UserType,
        string AuthorName,
        string Content,
        DateTime SendDate
    );
}
