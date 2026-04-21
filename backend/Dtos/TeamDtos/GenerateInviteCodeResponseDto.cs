using backend.Models;

namespace backend.Dtos.TeamDtos
{
    public record GenerateInviteCodeResponseDto(
        Guid CodeId,
        string Code,
        Guid TeamId,
        DateTime ExpiryDate
    );
}
    