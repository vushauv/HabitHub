using backend.Enums;
using backend.Dtos.TeamDtos;

namespace backend.Service.Interfaces
{
    public interface ITeamService
    {
        public Task<CreateTeamResponseDto> CreateTeam(Guid userId, CreateTeamRequestDto request);
        public Task<InviteCodeDto> GenerateInviteCode(Guid userId, Guid TeamId);
        public Task InvalidateInviteCode(Guid userId, Guid teamId, Guid codeId);
        public Task<JoinTeamResponseDto> JoinTeam(Guid userId, JoinTeamRequestDto request);
        public Task KickUser(Guid userId, Guid teamId, Guid memberId);
        public Task LeaveTeam(Guid userId, Guid teamId);
        public Task DeleteTeam(Guid userId, Guid teamId);
        public Task<List<TeamSummaryDto>> GetTeams(Guid userId, UserType userType);
        public Task<TeamSummaryDto> GetTeam(Guid userId, UserType userType, Guid teamId);
        public Task<List<TeamMemberDto>> GetTeamMembers(Guid userId, UserType userType, Guid teamId);
        public Task<List<InviteCodeDto>> GetActiveInviteCodes(Guid userId, Guid teamId);
    }
}
