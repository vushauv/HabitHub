using backend.Enums;
using backend.Dtos.TeamDtos;

namespace backend.Service
{
    public interface ITeamService
    {
        Task<CreateTeamResponseDto> CreateTeam(Guid userId, CreateTeamRequestDto request);
        Task<GenerateInviteCodeResponseDto> GenerateInviteCode(Guid userId, Guid TeamId);
        Task InvalidateInviteCode(Guid userId, Guid teamId, Guid codeId);
        Task<JoinTeamResponseDto> JoinTeam(Guid userId, JoinTeamRequestDto request);
        Task KickUser(Guid userId, Guid teamId, Guid memberId);
        Task LeaveTeam(Guid userId, Guid teamId);
        Task DeleteTeam(Guid userId, Guid teamId);
    }
}
