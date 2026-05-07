using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.HabitEntryDtos
{
    public record LeaderboardResponseDto(
        Guid MemberId,
        string MemberName,
        double? TotalValue,
        int LoggedCount,
        int Rank
    );
}