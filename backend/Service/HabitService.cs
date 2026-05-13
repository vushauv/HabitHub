using backend.Dtos.HabitDtos;
using backend.Dtos.HabitEntryDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Service.Interfaces;

namespace backend.Service
{
    public class HabitService(
        IHabitRepository habits, 
        IHabitTeamRepository habitTeams, 
        IMembershipRepository memberships,
        IHabitEntryRepository habitEntries,
        ITeamMemberRepository members
    ):IHabitService
    {
        public async Task<CreateHabitResponseDto> CreateHabit(Guid userId, Guid teamId, CreateHabitRequestDto request)
        {
            string name = NormalizeString(request.Name);
            string? goal = NormalizeNullableString(request.Goal);

            HabitType habitType = request.HabitType ?? HabitType.Binary;

            if (habitType == HabitType.Binary && request.Unit != null)
                throw new RequestValidationException("Unit is allowed only for quantitative habits.");

            if (habitType == HabitType.Quantitative && request.Unit == null)
                throw new RequestValidationException("Unit is required for quantitative habits.");

            if (request.ExpiryDate != null && request.ExpiryDate <= DateTime.UtcNow)
                throw new RequestValidationException("Expiry date must be in the future.");

            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            Habit habit = new Habit
            {
                HabitId = Guid.NewGuid(),
                TeamId = team.TeamId,
                CreatorId = userId,
                Name = name,
                Goal = goal,
                HabitType = habitType,
                Unit = request.Unit,
                ExpiryDate = request.ExpiryDate,
                HabitState = HabitState.Active
            };

            Habit createdHabit = await habits.CreateHabitAsync(habit);

            return new CreateHabitResponseDto(
                createdHabit.HabitId,
                createdHabit.TeamId,
                createdHabit.Name,
                createdHabit.Goal,
                createdHabit.CreatorId,
                createdHabit.HabitState,
                createdHabit.HabitType,
                createdHabit.Unit,
                createdHabit.ExpiryDate
            );
        }
        public async Task<List<HabitSummaryDto>> GetTeamHabits(Guid userId, UserType userType, Guid teamId, HabitState state)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException();

            if (userType == UserType.Creator)
            {
                bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, userId);
                if (!isTeamCreator)
                    throw new ForbiddenException();
            }
            else if (userType == UserType.Member)
            {
                TeamMember? member = await members.GetMemberByIdAsync(userId);
                if (member == null)
                    throw new ForbiddenException();

                bool isActiveMember = await memberships.IsActiveMembershipAsync(team.TeamId, member.MemberId);
                if (!isActiveMember)
                    throw new ForbiddenException();
            }
            else
                throw new AuthRequiredException();

            List<Habit> teamHabits;

            if (state == HabitState.Active)
            {
                teamHabits = await habits.GetActiveHabitsByTeamIdAsync(team.TeamId);
            }
            else if (state == HabitState.Archived)
            {
                teamHabits = await habits.GetArchivedHabitsByTeamIdAsync(team.TeamId);
            }
            else
                throw new RequestValidationException("Only Active or Archived habits can be listed.");

            return teamHabits.Select(h => new HabitSummaryDto(
                h.HabitId,
                h.Name,
                h.Goal,
                h.HabitState,
                h.HabitType,
                h.Unit,
                h.ExpiryDate
            )).ToList();
        }

        public async Task<HabitSummaryDto> EditHabit(Guid userId, Guid habitId, EditHabitRequestDto request)
        {
            if (request.ClearGoal && NormalizeNullableString(request.Goal) != null)
                throw new RequestValidationException("Cannot define goal and clear goal at the same time.");

            if (request.ClearExpiryDate && request.ExpiryDate != null)
                throw new RequestValidationException("Cannot define expiry date and clear expiry date at the same time.");

            if (request.Name != null && NormalizeString(request.Name).Length == 0)
                throw new RequestValidationException("Habit name cannot be empty.");

            if(request.ExpiryDate != null && request.ExpiryDate <= DateTime.UtcNow)
                throw new RequestValidationException("Expiry date must be in the future");

            Habit? habit = await habits.GetHabitByIdAsync(habitId);
            if (habit == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(habit.TeamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            if (habit.HabitState == HabitState.Archived) 
                throw new ConflictException("habit-archived", "Habit is archived.");

            if (request.Name != null)
            {
                string name = NormalizeString(request.Name);
                habit.Name = name;
            }

            if (request.ClearGoal)
                habit.Goal = null;
            else if (request.Goal != null)
                habit.Goal = NormalizeNullableString(request.Goal);

            if (request.ClearExpiryDate)
                habit.ExpiryDate = null;
            else if(request.ExpiryDate != null)
                habit.ExpiryDate = request.ExpiryDate;

            await habits.UpdateHabitAsync(habit);

            return new HabitSummaryDto(
                habit.HabitId,
                habit.Name,
                habit.Goal,
                habit.HabitState,
                habit.HabitType,
                habit.Unit,
                habit.ExpiryDate
            );
        }
        public async Task ArchiveHabit(Guid userId, Guid habitId)
        {
            Habit? habit = await habits.GetHabitByIdAsync(habitId);
            if (habit == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(habit.TeamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            if (habit.HabitState == HabitState.Archived)
                return;

            bool archived = await habits.ArchiveHabitAsync(habit.HabitId);
            if (!archived)
                throw new NotFoundException();
        }

        public async Task DeleteHabit(Guid userId, Guid habitId)
        {
            Habit? habit = await habits.GetHabitByIdAsync(habitId);
            if (habit == null)
                throw new NotFoundException();

            bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(habit.TeamId, userId);
            if (!isTeamCreator)
                throw new ForbiddenException();

            bool deleted = await habits.DeleteHabitAsync(habit.HabitId);
            if (!deleted)
                throw new NotFoundException();
        }

        public async Task<HabitSummaryDto> GetHabit(Guid userId, UserType userType, Guid habitId) 
        {
            Habit? habit = await habits.GetHabitByIdAsync(habitId);
            if (habit == null)
                throw new NotFoundException();

            if(userType == UserType.Creator)
            {
                bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(habit.TeamId, userId);
                if (!isTeamCreator)
                    throw new ForbiddenException();
            }
            else if(userType == UserType.Member)
            {
                bool isActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, userId);
                if (!isActiveMember)
                    throw new ForbiddenException();
            }
            else
                throw new AuthRequiredException();

            return new HabitSummaryDto(
                habit.HabitId,
                habit.Name,
                habit.Goal,
                habit.HabitState,
                habit.HabitType,
                habit.Unit,
            habit.ExpiryDate
            );
        }

        public async Task<HabitEntryResponseDto> LogProgress(Guid userId, UserType userType, Guid habitId, LogProgressRequestDto request)
        {
            if (userType != UserType.Member)
                throw new ForbiddenException();

            if (request.Status != EntryStatus.Logged && request.Status != EntryStatus.Skipped)
                throw new RequestValidationException("Status must be Logged or Skipped.");

            if (request.Status == EntryStatus.Skipped && request.Value != null)
                throw new RequestValidationException("Value is not allowed when status is Skipped.");

            string? notes = NormalizeNullableString(request.Notes);

            Habit? habit = await habits.GetHabitByIdAsync(habitId);
            if (habit == null)
                throw new NotFoundException();

            TeamMember? member = await members.GetMemberByIdAsync(userId);
            if (member == null)
                throw new ForbiddenException();

            bool isActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, member.MemberId);
            if (!isActiveMember)
                throw new ForbiddenException();

            if (habit.HabitState == HabitState.Archived)
                throw new ConflictException("habit-archived", "Habit is archived.");

            if (request.Status == EntryStatus.Logged)
            {
                if (habit.HabitType == HabitType.Binary && request.Value != null)
                    throw new RequestValidationException("Value is not allowed for binary habits.");

                if (habit.HabitType == HabitType.Quantitative && request.Value == null)
                    throw new RequestValidationException("Value is required for quantitative habits.");
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            HabitEntry? existingEntry = await habitEntries.GetHabitEntryByHabitMemberLogDateAsync(
                habit.HabitId,
                member.MemberId,
                today
            );

            if (existingEntry!=null)
                throw new ConflictException("log-already-exists", "Log already exists for this habit for today.");

            HabitEntry entry = new HabitEntry
            {
                EntryId = Guid.NewGuid(),
                HabitId = habit.HabitId,
                MemberId = member.MemberId,
                LogDate = today,
                LoggedAt = DateTime.UtcNow,
                Status = request.Status,
                Value = request.Status == EntryStatus.Skipped ? null : request.Value,
                Notes = notes
            };

            HabitEntry createdEntry = await habitEntries.CreateHabitEntryAsync(entry);

            return new HabitEntryResponseDto(
                createdEntry.EntryId,
                createdEntry.HabitId,
                createdEntry.MemberId,
                createdEntry.LoggedAt,
                createdEntry.LogDate,
                createdEntry.Status,
                createdEntry.Value,
                createdEntry.Notes
            );
        }
        public async Task UndoLog(Guid userId, UserType userType, Guid habitId, Guid entryId)
        {
            if (userType != UserType.Member)
                throw new ForbiddenException();

            Habit? habit = await habits.GetHabitByIdAsync(habitId);
            if (habit == null)
                throw new NotFoundException();

            TeamMember? member = await members.GetMemberByIdAsync(userId);
            if (member == null)
                throw new ForbiddenException();

            bool isActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, member.MemberId);
            if (!isActiveMember)
                throw new ForbiddenException();

            if (habit.HabitState == HabitState.Archived)
                throw new ConflictException("habit-archived", "Habit is archived.");

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            HabitEntry? entry = await habitEntries.GetHabitEntryByHabitMemberLogDateAsync(habitId, member.MemberId , today);
            if (entry == null || entry.EntryId != entryId)
                throw new NotFoundException("log-not-found", "Log not found.");

            bool undone = await habitEntries.DeleteHabitEntryAsync(entry.EntryId);
            if (!undone)
                throw new NotFoundException("log-not-found", "Log not found."); 
        }
        public async Task<List<HabitEntryResponseDto>> ViewProgress(Guid userId, UserType userType, Guid habitId, Guid? memberId)
        {
            if(userType != UserType.Member)
                throw new ForbiddenException();

            Habit? habit = await habits.GetHabitByIdAsync(habitId);
            if (habit == null)
                throw new NotFoundException();
            
            TeamMember? member = await members.GetMemberByIdAsync(userId);
            if (member == null)
                throw new ForbiddenException();

            bool isActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, member.MemberId);
            if (!isActiveMember)
                throw new ForbiddenException();
        
            List<HabitEntry> entries;
            if (memberId != null)
            {
                bool targetIsActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, memberId.Value);
                if (!targetIsActiveMember)
                    throw new NotFoundException();

                entries = await habitEntries.GetHabitEntriesByHabitAndMemberAsync(habit.HabitId, memberId.Value);
            }
            else
                entries = await habitEntries.GetHabitEntriesByHabitAndMemberAsync(habit.HabitId, member.MemberId);

            return entries.Select(e => new HabitEntryResponseDto(
                e.EntryId,
                e.HabitId,
                e.MemberId,
                e.LoggedAt,
                e.LogDate,
                e.Status,
                e.Value,
                e.Notes
            )).ToList();
        }
        public async Task<TodayHabitEntryStatusDto> GetMyTodayEntryStatus(Guid userId, UserType userType, Guid habitId)
        {
            if (userType != UserType.Member)
                throw new ForbiddenException();

            Habit? habit = await habits.GetHabitByIdAsync(habitId);
            if (habit == null)
                throw new NotFoundException();

            TeamMember? member = await members.GetMemberByIdAsync(userId);
            if (member == null)
                throw new ForbiddenException();

            bool isActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, member.MemberId);
            if (!isActiveMember)
                throw new ForbiddenException();

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            HabitEntry? entry = await habitEntries.GetHabitEntryByHabitMemberLogDateAsync(habitId, member.MemberId, today);
            if (entry == null)
                return new TodayHabitEntryStatusDto(EntryStatus.Pending, null);

            return new TodayHabitEntryStatusDto(
                entry.Status,
                new HabitEntryResponseDto(
                    entry.EntryId,
                    entry.HabitId,
                    entry.MemberId,
                    entry.LoggedAt,
                    entry.LogDate,
                    entry.Status,
                    entry.Value,
                    entry.Notes
                )
            );
        }
        public async Task<List<LeaderboardResponseDto>> ViewLeaderboard(Guid userId, UserType userType, Guid habitId)
        {
            Habit? habit = await habits.GetHabitByIdAsync(habitId);
            if (habit == null)
                throw new NotFoundException();

            if (userType == UserType.Creator)
            {
                bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(habit.TeamId, userId);
                if (!isTeamCreator)
                    throw new ForbiddenException();
            }
            else if (userType == UserType.Member)
            {
                TeamMember? member = await members.GetMemberByIdAsync(userId);
                if (member == null)
                    throw new ForbiddenException();

                bool isActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, member.MemberId);
                if (!isActiveMember)
                    throw new ForbiddenException();
            }
            else
                throw new AuthRequiredException();

            List<HabitEntry> entries = await habitEntries.GetHabitEntriesForLeaderboardAsync(habit.HabitId);
            List<TeamMember> teamMembers = await members.GetMembersByHabitEntriesAsync(habit.HabitId);

            List<LeaderboardRowDto> sortedResults = entries.GroupBy(e => e.MemberId).Select(group =>
            {
                TeamMember? member = teamMembers.FirstOrDefault(m => m.MemberId == group.Key);

                int loggedCount = group.Count(e => e.Status == EntryStatus.Logged);

                double totalValue = habit.HabitType == HabitType.Quantitative
                    ? group
                    .Where(e => e.Status == EntryStatus.Logged && e.Value != null)
                    .Sum(e => (double)e.Value!.Value)
                    : loggedCount;

                return new LeaderboardRowDto(
                    group.Key,
                    member?.Name ?? "Unknown",
                    loggedCount,
                    totalValue
                );
            })
            .OrderByDescending(e => e.TotalValue)
            .ThenByDescending(e => e.LoggedCount)
            .ToList();

            return sortedResults.Select((entry, index) => new LeaderboardResponseDto(
                entry.MemberId,
                entry.MemberName,
                entry.TotalValue,
                entry.LoggedCount,
                index + 1
            )).ToList();
        }

        private static string NormalizeString(string s) => s.Trim();
        private static string? NormalizeNullableString(string? s)
        {
            string normalized = s?.Trim() ?? "";
            return normalized.Length == 0 ? null : normalized;
        }
    }
}
