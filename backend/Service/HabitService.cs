using backend.Dtos.HabitDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Models;
using backend.Repositories;

namespace backend.Service
{
    public class HabitService(IHabitRepository habits, IHabitTeamRepository habitTeams, IMembershipRepository memberships):IHabitService
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

            if (habit.HabitState == HabitState.Archived) //UPDATED
                throw new ConflictException("habit-archived", "Habit is archived.");
            //UPDATED No habit-closed error exists

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

        public async Task<HabitSummaryDto> GetHabit(Guid userId, UserType userType, Guid habitId) //UPDATED:
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

        private static string NormalizeString(string s) => s.Trim();
        private static string? NormalizeNullableString(string? s)
        {
            string normalized = s?.Trim() ?? "";
            return normalized.Length == 0 ? null : normalized;
        }
    }
}
