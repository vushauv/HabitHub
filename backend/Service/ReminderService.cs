using backend.Data.UnitOfWork;
using backend.Dtos.ReminderDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Service.Interfaces;

namespace backend.Service
{
    public class ReminderService(
        IHabitRepository habits,
        IHabitTeamRepository habitTeams,
        IMembershipRepository memberships,
        ITeamMemberRepository members,
        IReminderRepository reminders,
        IUnitOfWork unitOfWork,
        ILogger<ReminderService> logger
    ) : IReminderService
    {
        public async Task<HabitReminderResponseDto> SetHabitReminder(Guid userId, UserType userType, Guid habitId, SetReminderRequestDto request)
        {
            if (userType != UserType.Creator)
            {
                logger.LogWarning("Set reminder rejected: user {UserId} is not creator", userId);
                throw new ForbiddenException();
            }

            Habit habit = await GetHabitOrThrow(habitId);

            if (habit.HabitState != HabitState.Active)
            {
                logger.LogWarning("Set reminder rejected: habit {HabitId} is inactive", habitId);
                throw new ConflictException("habit-archived", "Cannot set reminders for inactive habit.");
            }

            bool ownsTeam = await habitTeams.CheckOwnershipOfTeamAsync(habit.TeamId, userId);
            if (!ownsTeam)
            {
                logger.LogWarning("Set reminder rejected: user {UserId} is not owner of team {TeamId}", userId, habit.TeamId);
                throw new ForbiddenException();
            }

            HabitReminderResponseDto response = await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                bool updated = await reminders.SetHabitReminderTimeAsync(habit.HabitId, request.ReminderTime);
                if (!updated)
                {
                    logger.LogWarning("Set reminder rejected: habit {HabitId} not found", habit.HabitId);
                    throw new NotFoundException();
                }

                List<Membership> activeMemberships = await memberships.GetActiveMembershipsByTeamIdAsync(habit.TeamId);
                List<Guid> memberIds = activeMemberships.Select(m => m.MemberId).ToList();

                await reminders.CreateMissingRemindersForHabitAsync(habit.HabitId, memberIds);
                logger.LogInformation("Ensured reminder settings for habit {HabitId} and {MemberCount} members", habit.HabitId, memberIds.Count);

                return new HabitReminderResponseDto(habit.HabitId, request.ReminderTime);
            });

            logger.LogInformation("Set reminder time {ReminderTime} for habit {HabitId} by creator {UserId}", response.ReminderTime, response.HabitId, userId);
            return response;
        }

        public async Task ClearHabitReminder(Guid userId, UserType userType, Guid habitId)
        {
            if (userType != UserType.Creator)
            {
                logger.LogWarning("Clear reminder rejected: user {UserId} is not creator", userId);
                throw new ForbiddenException();
            }

            Habit habit = await GetHabitOrThrow(habitId);

            bool ownsTeam = await habitTeams.CheckOwnershipOfTeamAsync(habit.TeamId, userId);
            if (!ownsTeam)
            {
                logger.LogWarning("Clear reminder rejected: user {UserId} is not owner of team {TeamId}", userId, habit.TeamId);
                throw new ForbiddenException();
            }

            bool cleared = await reminders.ClearHabitReminderTimeAsync(habit.HabitId);
            if (!cleared)
            {
                logger.LogWarning("Clear reminder rejected: habit {HabitId} not found", habit.HabitId);
                throw new NotFoundException();
            }
            logger.LogInformation("Cleared reminder time for habit {HabitId} by creator {UserId}", habit.HabitId, userId);
        }

        public async Task<MyReminderResponseDto> ChangeMyReminder(Guid userId, UserType userType, Guid habitId, ChangeMyReminderRequestDto request)
        {
            if (userType != UserType.Member)
            {
                logger.LogWarning("Change my reminder rejected: user {UserId} is not member", userId);
                throw new ForbiddenException();
            }

            Habit habit = await GetHabitOrThrow(habitId);

            TeamMember? member = await members.GetMemberByIdAsync(userId);
            if (member == null)
            {
                logger.LogWarning("Change my reminder rejected: member {MemberId} not found", userId);
                throw new ForbiddenException();
            }

            bool isActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, member.MemberId);
            if (!isActiveMember)
            {
                logger.LogWarning("Change my reminder rejected: member {MemberId} not active in team {TeamId}", member.MemberId, habit.TeamId);
                throw new ForbiddenException();
            }

            Reminder? reminder = await reminders.GetReminderByHabitAndMemberAsync(habit.HabitId, member.MemberId);

            if (reminder == null)
            {
                reminder = new Reminder
                {
                    ReminderId = Guid.NewGuid(),
                    HabitId = habit.HabitId,
                    MemberId = member.MemberId,
                    Enabled = request.Enabled,
                    LastSentAt = null
                };

                await reminders.CreateReminderAsync(reminder);
                logger.LogInformation("Created reminder setting {ReminderId} for habit {HabitId}, member {MemberId}", reminder.ReminderId, habit.HabitId, member.MemberId);
            }
            else
            {
                bool updated = await reminders.SetReminderEnabledAsync(habit.HabitId, member.MemberId, request.Enabled);

                if (!updated)
                {
                    logger.LogWarning("Change my reminder rejected: reminder for habit {HabitId}, member {MemberId} not found", habit.HabitId, member.MemberId);
                    throw new NotFoundException();
                }

                reminder.Enabled = request.Enabled;
            }

            logger.LogInformation("Changed reminder setting for habit {HabitId}, member {MemberId} to enabled {Enabled}", habit.HabitId, member.MemberId, reminder.Enabled);
            return new MyReminderResponseDto(habit.HabitId, member.MemberId, reminder.Enabled, habit.ReminderTime);
        }

        public async Task<MyReminderResponseDto> GetMyReminder(Guid userId, UserType userType, Guid habitId)
        {
            if (userType != UserType.Member)
            {
                logger.LogWarning("Get my reminder rejected: user {UserId} is not member", userId);
                throw new ForbiddenException();
            }

            Habit habit = await GetHabitOrThrow(habitId);

            TeamMember? member = await members.GetMemberByIdAsync(userId);
            if (member == null)
            {
                logger.LogWarning("Get my reminder rejected: member {MemberId} not found", userId);
                throw new ForbiddenException();
            }

            bool isActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, member.MemberId);
            if (!isActiveMember)
            {
                logger.LogWarning("Get my reminder rejected: member {MemberId} not active in team {TeamId}", member.MemberId, habit.TeamId);
                throw new ForbiddenException();
            }

            Reminder? reminder = await reminders.GetReminderByHabitAndMemberAsync(habit.HabitId, member.MemberId);

            logger.LogInformation("Returned reminder setting for habit {HabitId}, member {MemberId}", habit.HabitId, member.MemberId);
            return new MyReminderResponseDto(habit.HabitId, member.MemberId, reminder?.Enabled ?? true, habit.ReminderTime);
        }

        private async Task<Habit> GetHabitOrThrow(Guid habitId)
        {
            Habit? habit = await habits.GetHabitByIdAsync(habitId);

            if (habit == null)
            {
                logger.LogWarning("Habit {HabitId} not found", habitId);
                throw new NotFoundException();
            }

            return habit;
        }
    }
}