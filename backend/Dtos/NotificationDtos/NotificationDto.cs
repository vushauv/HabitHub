using backend.Enums;

namespace backend.Dtos.NotificationDtos
{
    public record NotificationDto(
        Guid NotificationId, 
        string Content, 
        DateTime CreatedAt, 
        NotificationStatus Status, 
        NotificationType Type
    );
}