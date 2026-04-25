using backend.Dtos.TeamDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Models;
using backend.Repositories;
using backend.Utils;

namespace backend.Service
{
    public class TeamService(
        IHabitTeamRepository habitTeams,
        ITeamMemberRepository members,
        IMembershipRepository memberships,
        ITeamCreatorRepository creators,
        IInviteCodeRepository inviteCodes
        ): ITeamService
    {
        public async Task<CreateTeamResponseDto> CreateTeam(Guid userId, CreateTeamRequestDto request)
        {
            string name = NormalizeString(request.Name);
            if (name.Length == 0)
                throw new RequestValidationException("Team name is required.");
            
            TeamCreator? creator = await creators.GetCreatorByIdAsync(userId);
            if (creator == null)
                throw new ForbiddenException();

            HabitTeam team = new HabitTeam
            {
                TeamId = Guid.NewGuid(),
                Name = name,
                CreatorId = creator.CreatorId
            };

            HabitTeam createdTeam = await habitTeams.CreateHabitTeamAsync(team);
            return new CreateTeamResponseDto(createdTeam.TeamId, createdTeam.Name);
        }

        public async Task<InviteCodeDto> GenerateInviteCode(Guid userId, Guid teamId)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(teamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            await inviteCodes.InvalidateActiveInviteCodesByTeamIdAsync(team.TeamId);

            InviteCode inviteCode = new InviteCode
            {
                CodeId = Guid.NewGuid(),
                Code = InviteCodeGenerator.GenerateInviteCodeValue(),
                TeamId = team.TeamId,
                ExpiryDate = DateTime.UtcNow.AddDays(10), 
                Status = CodeStatus.Active
            };

            InviteCode createdInviteCode = await inviteCodes.CreateInviteCodeAsync(inviteCode);

            return new InviteCodeDto(
                createdInviteCode.CodeId,
                createdInviteCode.Code,
                createdInviteCode.TeamId,
                createdInviteCode.ExpiryDate
            );
        }

        public async Task InvalidateInviteCode(Guid userId, Guid teamId, Guid codeId)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(teamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            InviteCode? inviteCode = await inviteCodes.GetInviteCodeByIdAsync(codeId);
            if (inviteCode == null || inviteCode.TeamId != teamId)
                throw new NotFoundException();

            if (inviteCode.Status == CodeStatus.Active && inviteCode.ExpiryDate <= DateTime.UtcNow)
            {
                await inviteCodes.UpdateInviteCodeStatusAsync(inviteCode.CodeId, CodeStatus.Expired);
                throw new ConflictException(errorCode: "code-expired", "The invite code is expired.");
            }

            if(inviteCode.Status == CodeStatus.Expired)
                throw new ConflictException(errorCode: "code-expired", "The invite code is expired.");

            if (inviteCode.Status == CodeStatus.Invalid)
                throw new ConflictException("code-invalid", "The invite code is invalid.");

            await inviteCodes.UpdateInviteCodeStatusAsync(inviteCode.CodeId, CodeStatus.Invalid);
        }

        public async Task<JoinTeamResponseDto> JoinTeam(Guid userId, JoinTeamRequestDto request)
        {
            string codeValue = NormalizeString(request.Code);

            InviteCode? inviteCode = await inviteCodes.GetInviteCodeByCodeAsync(codeValue);
            if (inviteCode == null)
                throw new NotFoundException("code-not-found", "Invite code not found");

            if (inviteCode.Status == CodeStatus.Active && inviteCode.ExpiryDate <= DateTime.UtcNow)
            {
                await inviteCodes.UpdateInviteCodeStatusAsync(inviteCode.CodeId, CodeStatus.Expired);
                throw new ConflictException("code-expired", "Invite code has expired.");
            }

            if (inviteCode.Status == CodeStatus.Expired)
                throw new ConflictException("code-expired", "Invite code has expired.");

            if (inviteCode.Status == CodeStatus.Invalid)
                throw new ConflictException("code-invalid", "Invite code is invalid.");

            HabitTeam? habitTeam = await habitTeams.GetHabitTeamByIdAsync(inviteCode.TeamId);
            if (habitTeam == null)
                throw new NotFoundException(); 

            TeamMember? member = await members.GetMemberByIdAsync(userId);
            if (member == null)
                throw new ForbiddenException();

            Membership? membership = await memberships.GetMembershipByTeamIdAndMemberIdAsync(inviteCode.TeamId, member.MemberId);
            if (membership != null && membership.Status == MembershipStatus.Active)
                throw new ConflictException("already-member", "User is already a member of this team.");

            if(membership == null)
            {
                Membership createdMembership = new Membership
                {
                    MembershipId = Guid.NewGuid(),
                    TeamId = inviteCode.TeamId,
                    MemberId = member.MemberId,
                    Status = MembershipStatus.Active
                };
                await memberships.CreateMembershipAsync(createdMembership);
            }
            else
            {
                await memberships.UpdateMembershipStatusAsync(inviteCode.TeamId, member.MemberId, MembershipStatus.Active);
            }
            return new JoinTeamResponseDto(inviteCode.TeamId, member.MemberId);
        }
        public async Task KickUser(Guid userId, Guid teamId, Guid memberId)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            bool isActiveMembership = await memberships.IsActiveMembershipAsync(team.TeamId, memberId);
            if (!isActiveMembership)
                throw new NotFoundException();

            await memberships.UpdateMembershipStatusAsync(team.TeamId, memberId, MembershipStatus.Kicked);
        }
        public async Task LeaveTeam(Guid userId, Guid teamId)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            bool isActiveMembership = await memberships.IsActiveMembershipAsync(team.TeamId, userId);
            if (!isActiveMembership)
                throw new NotFoundException();

            await memberships.UpdateMembershipStatusAsync(team.TeamId, userId, MembershipStatus.Left);
        }
        public async Task DeleteTeam(Guid userId, Guid teamId)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            await inviteCodes.InvalidateActiveInviteCodesByTeamIdAsync(team.TeamId); //TODO: Discuss, not in sequence but i believe useful
            await habitTeams.DeleteHabitTeamAsync(team.TeamId);
        }
        public async Task<List<TeamSummaryDto>> GetTeams(Guid userId, UserType userType)
        {
            List<TeamSummaryDto> results = new List<TeamSummaryDto>();

            if(userType == UserType.Creator)
            {
                List<HabitTeam> teams = await habitTeams.GetAllHabitTeamsByCreatorAsync(userId);
                results.AddRange(teams.Select(t => new TeamSummaryDto(t.TeamId, t.Name)));
            }

            if(userType == UserType.Member)
            {
                List<Membership> activeMemberships = await memberships.GetActiveMembershipsByMemberIdAsync(userId);
                List<Guid> teamIds = activeMemberships.Select(m => m.TeamId).ToList();

                List<HabitTeam> teams = await habitTeams.GetHabitTeamsByIdsAsync(teamIds);

                results.AddRange(teams.Select(t => new TeamSummaryDto(t.TeamId, t.Name)));
            }

            return results;
        }
        public async Task<TeamDetailsDto> GetTeam(Guid userId, UserType userType, Guid teamId)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            if(userType == UserType.Member)
            {
                bool isActiveMember = await memberships.IsActiveMembershipAsync(teamId, userId);
                if (!isActiveMember)
                    throw new ForbiddenException();
            }
            if (userType == UserType.Creator)
            {
                bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, userId);
                if (!isTeamCreator)
                    throw new ForbiddenException();
            }
            return new TeamDetailsDto(team.TeamId, team.Name);
        }
        public async Task<List<TeamMemberDto>> GetTeamMembers(Guid userId, UserType userType, Guid teamId)
        {
            List<TeamMemberDto> results = new List<TeamMemberDto>();

            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            if (userType == UserType.Member)
            {
                bool isActiveMember = await memberships.IsActiveMembershipAsync(teamId, userId);
                if (!isActiveMember)
                    throw new ForbiddenException();
            }
            if (userType == UserType.Creator)
            {
                bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, userId);
                if (!isTeamCreator)
                    throw new ForbiddenException();
            }

            List<Membership> membershipList = await memberships.GetMembershipsByTeamIdAsync(teamId);
            List<Guid> memberIds = membershipList.Select(m => m.MemberId).ToList();
            List<TeamMember> membersList = await members.GetMembersByIdsAsync(memberIds);

            results = membershipList.Select(m =>
            {
                TeamMember? member = membersList.FirstOrDefault(mem => mem.MemberId == m.MemberId);
                return new TeamMemberDto(m.MemberId, member?.Name ?? "Unknown", member?.Email ?? "Unknown", m.Status);
            }).ToList();

            return results;
        }
        public async Task<List<InviteCodeDto>> GetActiveInviteCodes(Guid userId, Guid teamId)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            List<InviteCode> activeCodes = await inviteCodes.GetActiveInviteCodesByTeamIdAsync(teamId);

            return activeCodes.Select(code =>
            {
                return new InviteCodeDto(code.CodeId, code.Code, code.TeamId, code.ExpiryDate);
            }).ToList();
        }

        private static string NormalizeString(string name) => name.Trim();
    }
}
