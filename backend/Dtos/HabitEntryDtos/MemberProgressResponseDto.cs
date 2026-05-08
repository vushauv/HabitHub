using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.HabitEntryDtos
{
    public record MemberProgressResponseDto(
        Guid MemberId,
        string MemberName,
        List<HabitEntryResponseDto> Entries
    );
}