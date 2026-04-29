using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.AuthDtos
{
    public record MemberProgressResponseDto(
        Guid MemberId,
        string MemberName,
        List<HabitEntryResponseDto> Entries
    );
}