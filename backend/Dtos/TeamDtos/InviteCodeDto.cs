namespace backend.Dtos.TeamDtos
{
    public record InviteCodeDto(
        Guid CodeId,
        string Code,
        Guid TeamId,
        DateTime ExpiryDate
    );
}
