using backend.Enums;
using backend.Dtos.TeamDtos;

namespace backend.Service
{
    public interface ITeamService
    {
        Task<CreateTeamResponseDto> CreateTeam(Guid userId, CreateTeamRequestDto request);
        Task<InviteCodeDto> GenerateInviteCode(Guid userId, Guid TeamId);
        Task InvalidateInviteCode(Guid userId, Guid teamId, Guid codeId);
        Task<JoinTeamResponseDto> JoinTeam(Guid userId, JoinTeamRequestDto request);
        Task KickUser(Guid userId, Guid teamId, Guid memberId);
        Task LeaveTeam(Guid userId, Guid teamId);
        Task DeleteTeam(Guid userId, Guid teamId);
        Task<List<TeamSummaryDto>> GetTeams(Guid userId, UserType userType);
        Task<TeamDetailsDto> GetTeam(Guid userId, UserType userType, Guid teamId);
        Task<List<TeamMemberDto>> GetTeamMembers(Guid userId, UserType userType, Guid teamId);
        Task<List<InviteCodeDto>> GetActiveInviteCodes(Guid userId, Guid teamId);
    }
}
