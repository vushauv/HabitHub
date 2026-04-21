namespace backend.Dtos.TeamDtos
{
    public record JoinTeamResponseDto(
        Guid TeamId,
        Guid MemberId
    );
}
