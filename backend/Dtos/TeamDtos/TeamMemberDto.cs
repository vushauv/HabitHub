using backend.Enums;

namespace backend.Dtos.TeamDtos
{
    public record TeamMemberDto(
        Guid MemberId,
        string Name,
        string Email, 
        MembershipStatus Status
    );
}
