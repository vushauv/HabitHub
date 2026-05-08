using System.Transactions;

namespace backend.Dtos.HabitEntryDtos
{
    public record LeaderboardRowDto
    (
        Guid MemberId,
        string MemberName,
        int LoggedCount,
        double? TotalValue
    );
}
