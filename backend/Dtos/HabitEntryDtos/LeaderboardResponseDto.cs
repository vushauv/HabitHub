using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.AuthDtos
{
    public record LeaderboardResponseDto(
        Guid MemberId,
        string MemberName,
        float? TotalValue,
        int LoggedCount,
        int SkippedCount,
        int Rank
    );
}