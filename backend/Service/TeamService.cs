using backend.Dtos.TeamDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Models;
using backend.Repositories;
using backend.Utils;
using System.ComponentModel;

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

            //Membership creatorMembership = new Membership
            //{
            //    MembershipId = Guid.NewGuid(),
            //    TeamId = createdTeam.TeamId,
            //    MemberId = creator.CreatorId,
            //    Status = MembershipStatus.Active
            //};

            //await memberships.CreateMembershipAsync(creatorMembership);

            return new CreateTeamResponseDto(createdTeam.TeamId, createdTeam.Name);
        }

        public async Task<GenerateInviteCodeResponseDto> GenerateInviteCode(Guid userId, Guid teamId)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(teamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            List<InviteCode> activeInviteCodes = await inviteCodes.GetActiveInviteCodesByTeamIdAsync(team.TeamId);
            foreach (InviteCode ic in activeInviteCodes)
                await inviteCodes.UpdateInviteCodeStatusAsync(ic.CodeId, CodeStatus.Invalid);

            InviteCode inviteCode = new InviteCode
            {
                CodeId = Guid.NewGuid(),
                Code = InviteCodeGenerator.GenerateInviteCodeValue(),
                TeamId = team.TeamId,
                ExpiryDate = DateTime.UtcNow.AddDays(10), 
                Status = CodeStatus.Active
            };

            InviteCode createdInviteCode = await inviteCodes.CreateInviteCodeAsync(inviteCode);

            return new GenerateInviteCodeResponseDto(
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
                throw new NotFoundException(); //TODO: Discuss during next sprint - this check is not in sequence but i believe its important (team can be deleted in between) 

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

            if (memberId == userId)
                throw new ConflictException("cannot-kick-self", "Team Creator cannot kick himself."); //TODO: Discuss during next sprint - this check doesnt make sense 

            TeamMember? member = await members.GetMemberByIdAsync(memberId);
            if (member == null)
                throw new NotFoundException(); //TODO: Discuss during next sprint - this is an additional check not specified in sequence diagram but I think useful

            bool isActiveMembership = await memberships.IsActiveMembershipAsync(team.TeamId, member.MemberId);
            if (!isActiveMembership)
                throw new NotFoundException();

            await memberships.UpdateMembershipStatusAsync(team.TeamId, member.MemberId, MembershipStatus.Kicked);
        }
        public async Task LeaveTeam(Guid userId, Guid teamId)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            TeamMember? member = await members.GetMemberByIdAsync(userId);
            if (member == null)
                throw new NotFoundException(); //TODO: Discuss during next sprint - this is an additional check not specified in sequence diagram but I think useful

            bool isActiveMembership = await memberships.IsActiveMembershipAsync(team.TeamId, member.MemberId);
            if (!isActiveMembership)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, userId);
            if (isTeamCreator)
                throw new ConflictException("creator-cannot-leave", "Team Creator cannot leave the team."); //TODO: Discuss during next sprint - this check doesnt make sense if we assume team creator doesnt have a membership

            await memberships.UpdateMembershipStatusAsync(team.TeamId, member.MemberId, MembershipStatus.Left);
        }
        public async Task DeleteTeam(Guid userId, Guid teamId)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            await habitTeams.DeleteHabitTeamAsync(team.TeamId);
        }

        private static string NormalizeString(string name) => name.Trim();
    }
}
