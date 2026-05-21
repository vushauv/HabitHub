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
        IReminderRepository reminders
    ) : IReminderService
    {
        public async Task<HabitReminderResponseDto> SetHabitReminder(Guid userId, UserType userType, Guid habitId, SetReminderRequestDto request)
        {
            if (userType != UserType.Creator)
                throw new ForbiddenException();

            Habit habit = await GetHabitOrThrow(habitId);

            if (habit.HabitState != HabitState.Active)
                throw new ConflictException("habit-archived", "Cannot set reminders for inactive habit.");

            bool ownsTeam = await habitTeams.CheckOwnershipOfTeamAsync(habit.TeamId, userId);
            if (!ownsTeam)
                throw new ForbiddenException();

            bool updated = await reminders.SetHabitReminderTimeAsync(habit.HabitId, request.ReminderTime);
            if (!updated)
                throw new NotFoundException();

            List<Membership> activeMemberships = await memberships.GetActiveMembershipsByTeamIdAsync(habit.TeamId);
            List<Guid> memberIds = activeMemberships.Select(m => m.MemberId).ToList();

            await reminders.CreateMissingRemindersForHabitAsync(habit.HabitId, memberIds);

            return new HabitReminderResponseDto(habit.HabitId, request.ReminderTime);
        }

        public async Task ClearHabitReminder(Guid userId, UserType userType, Guid habitId)
        {
            if (userType != UserType.Creator)
                throw new ForbiddenException();

            Habit habit = await GetHabitOrThrow(habitId);

            bool ownsTeam = await habitTeams.CheckOwnershipOfTeamAsync(habit.TeamId, userId);
            if (!ownsTeam)
                throw new ForbiddenException();

            bool cleared = await reminders.ClearHabitReminderTimeAsync(habit.HabitId);
            if (!cleared)
                throw new NotFoundException();
        }

        public async Task<MyReminderResponseDto> ChangeMyReminder(Guid userId, UserType userType, Guid habitId, ChangeMyReminderRequestDto request)
        {
            if (userType != UserType.Member)
                throw new ForbiddenException();

            Habit habit = await GetHabitOrThrow(habitId);

            TeamMember? member = await members.GetMemberByIdAsync(userId);
            if (member == null)
                throw new ForbiddenException();

            bool isActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, member.MemberId);
            if (!isActiveMember)
                throw new ForbiddenException();

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
            }
            else
            {
                bool updated = await reminders.SetReminderEnabledAsync(habit.HabitId, member.MemberId, request.Enabled);

                if (!updated)
                    throw new NotFoundException();

                reminder.Enabled = request.Enabled;
            }

            return new MyReminderResponseDto(habit.HabitId, member.MemberId, reminder.Enabled, habit.ReminderTime);
        }

        public async Task<MyReminderResponseDto> GetMyReminder(Guid userId, UserType userType, Guid habitId)
        {
            if (userType != UserType.Member)
                throw new ForbiddenException();

            Habit habit = await GetHabitOrThrow(habitId);

            TeamMember? member = await members.GetMemberByIdAsync(userId);
            if (member == null)
                throw new ForbiddenException();

            bool isActiveMember = await memberships.IsActiveMembershipAsync(habit.TeamId, member.MemberId);
            if (!isActiveMember)
                throw new ForbiddenException();

            Reminder? reminder = await reminders.GetReminderByHabitAndMemberAsync(habit.HabitId, member.MemberId);

            return new MyReminderResponseDto(habit.HabitId, member.MemberId, reminder?.Enabled ?? true, habit.ReminderTime);
        }

        private async Task<Habit> GetHabitOrThrow(Guid habitId)
        {
            Habit? habit = await habits.GetHabitByIdAsync(habitId);

            if (habit == null)
                throw new NotFoundException();

            return habit;
        }
    }
}