using backend.Data.UnitOfWork;
using backend.Enums;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Tests.Unit.Services.Habit;

public sealed class FakeHabitRepository : IHabitRepository
{
    public Dictionary<Guid, backend.Models.Habit> ById { get; } = new();
    public Dictionary<Guid, List<backend.Models.Habit>> ActiveByTeam { get; } = new();
    public Dictionary<Guid, List<backend.Models.Habit>> ArchivedByTeam { get; } = new();
    public backend.Models.Habit? LastCreated { get; private set; }
    public backend.Models.Habit? LastUpdated { get; private set; }
    public Guid? LastArchivedId { get; private set; }
    public Guid? LastDeletedId { get; private set; }
    public bool ArchiveResult { get; set; } = true;
    public bool DeleteResult { get; set; } = true;

    public Task<backend.Models.Habit> CreateHabitAsync(backend.Models.Habit habit)
    {
        LastCreated = habit;
        ById[habit.HabitId] = habit;
        return Task.FromResult(habit);
    }

    public Task<backend.Models.Habit?> GetHabitByIdAsync(Guid habitId)
        => Task.FromResult(ById.TryGetValue(habitId, out var h) ? h : null);

    public Task<List<backend.Models.Habit>> GetActiveHabitsByTeamIdAsync(Guid teamId)
        => Task.FromResult(ActiveByTeam.TryGetValue(teamId, out var l) ? l : new());

    public Task<List<backend.Models.Habit>> GetArchivedHabitsByTeamIdAsync(Guid teamId)
        => Task.FromResult(ArchivedByTeam.TryGetValue(teamId, out var l) ? l : new());

    public Task UpdateHabitAsync(backend.Models.Habit habit)
    {
        LastUpdated = habit;
        ById[habit.HabitId] = habit;
        return Task.CompletedTask;
    }

    public Task<bool> ArchiveHabitAsync(Guid habitId)
    {
        LastArchivedId = habitId;
        return Task.FromResult(ArchiveResult);
    }

    public Task<bool> DeleteHabitAsync(Guid habitId)
    {
        LastDeletedId = habitId;
        return Task.FromResult(DeleteResult);
    }

    public Task<backend.Models.Habit?> GetHabitWithEntriesByIdAsync(Guid habitId) => throw new NotImplementedException();
    public Task<List<backend.Models.Habit>> GetHabitsByTeamIdAsync(Guid teamId) => throw new NotImplementedException();
    public Task<List<Guid>> GetActiveHabitIdsWithReminderTimeByTeamIdAsync(Guid teamId) => throw new NotImplementedException();
    public Task<List<backend.Models.Habit>> GetHabitsByCreatorIdAsync(Guid creatorId) => throw new NotImplementedException();
    public Task UpdateHabitStateAsync(Guid habitId, HabitState state) => throw new NotImplementedException();
    public Task<bool> HabitExistsAsync(Guid habitId) => throw new NotImplementedException();
    public Task ArchiveExpiredActiveHabitsAsync() => throw new NotImplementedException();
}

public sealed class FakeHabitTeamRepository : IHabitTeamRepository
{
    public Dictionary<Guid, HabitTeam> TeamsById { get; } = new();
    public Dictionary<Guid, HabitTeam> TeamsByHabitId { get; } = new();

    public Dictionary<(Guid TeamId, Guid UserId), bool> Owners { get; } = new();

    public Task<HabitTeam?> GetHabitTeamByIdAsync(Guid teamId)
        => Task.FromResult(TeamsById.TryGetValue(teamId, out var t) ? t : null);

    public Task<HabitTeam?> GetHabitTeamByHabitIdAsync(Guid habitId)
        => Task.FromResult(TeamsByHabitId.TryGetValue(habitId, out var t) ? t : null);


    public Task<bool> CheckOwnershipOfTeamAsync(Guid teamId, Guid creatorId)
        => Task.FromResult(Owners.TryGetValue((teamId, creatorId), out var v) && v);

    public Task<HabitTeam> CreateHabitTeamAsync(HabitTeam team) => throw new NotImplementedException();
    public Task<List<HabitTeam>> GetAllHabitTeamsByCreatorAsync(Guid creatorId) => throw new NotImplementedException();
    public Task<List<HabitTeam>> GetHabitTeamsByIdsAsync(List<Guid> teamIds) => throw new NotImplementedException();
    public Task<bool> DeleteHabitTeamAsync(Guid teamId) => throw new NotImplementedException();
}

public sealed class FakeMembershipRepository : IMembershipRepository
{
    public Dictionary<(Guid TeamId, Guid MemberId), bool> Active { get; } = new();

    public Task<bool> IsActiveMembershipAsync(Guid teamId, Guid memberId)
        => Task.FromResult(Active.TryGetValue((teamId, memberId), out var v) && v);

    public Task<Membership> CreateMembershipAsync(Membership membership) => throw new NotImplementedException();
    public Task<Membership?> GetMembershipByTeamIdAndMemberIdAsync(Guid teamId, Guid memberId) => throw new NotImplementedException();
    public Task<Membership?> GetMembershipByIdAsync(Guid membershipId) => throw new NotImplementedException();
    public Task<List<Membership>> GetMembershipsByTeamIdAsync(Guid teamId) => throw new NotImplementedException();
    public Task<List<Membership>> GetActiveMembershipsByTeamIdAsync(Guid teamId) => throw new NotImplementedException();
    public Task<List<Membership>> GetActiveMembershipsByMemberIdAsync(Guid memberId) => throw new NotImplementedException();
    public Task<List<Membership>> GetMembershipsByMemberIdAsync(Guid memberId) => throw new NotImplementedException();
    public Task UpdateMembershipStatusAsync(Guid teamId, Guid memberId, MembershipStatus status) => throw new NotImplementedException();
}

public sealed class FakeHabitEntryRepository : IHabitEntryRepository
{
    public List<HabitEntry> Entries { get; } = new();
    public HabitEntry? LastCreated { get; private set; }
    public Guid? LastDeletedId { get; private set; }
    public bool DeleteResult { get; set; } = true;

    public Task<HabitEntry> CreateHabitEntryAsync(HabitEntry entry)
    {
        LastCreated = entry;
        Entries.Add(entry);
        return Task.FromResult(entry);
    }

    public Task<HabitEntry?> GetHabitEntryByHabitMemberLogDateAsync(Guid habitId, Guid memberId, DateOnly logDate)
        => Task.FromResult(Entries.FirstOrDefault(e => e.HabitId == habitId && e.MemberId == memberId && e.LogDate == logDate));

    public Task<List<HabitEntry>> GetHabitEntriesByHabitAndMemberAsync(Guid habitId, Guid memberId)
        => Task.FromResult(Entries.Where(e => e.HabitId == habitId && e.MemberId == memberId).ToList());

    public Task<List<HabitEntry>> GetHabitEntriesForLeaderboardAsync(Guid habitId)
        => Task.FromResult(Entries.Where(e => e.HabitId == habitId).ToList());

    public Task<bool> DeleteHabitEntryAsync(Guid entryId)
    {
        LastDeletedId = entryId;
        if (DeleteResult)
        {
            Entries.RemoveAll(e => e.EntryId == entryId);
        }
        return Task.FromResult(DeleteResult);
    }

    public Task<HabitEntry?> GetHabitEntryByIdAsync(Guid entryId) => throw new NotImplementedException();
    public Task<bool> HasHabitEntryForLogDateAsync(Guid habitId, Guid memberId, DateOnly logDate) => throw new NotImplementedException();
    public Task<List<HabitEntry>> GetHabitEntriesByHabitIdAsync(Guid habitId) => throw new NotImplementedException();
    public Task<List<HabitEntry>> GetHabitEntriesByMemberIdAsync(Guid memberId) => throw new NotImplementedException();
    public Task<List<HabitEntry>> GetHabitEntriesByHabitAndLogDateRangeAsync(Guid habitId, DateOnly from, DateOnly to) => throw new NotImplementedException();
    public Task<bool> DeleteHabitEntryByHabitMemberLogDateAsync(Guid habitId, Guid memberId, DateOnly logDate) => throw new NotImplementedException();
}

public sealed class FakeTeamMemberRepository : ITeamMemberRepository
{
    public Dictionary<Guid, TeamMember> ById { get; } = new();
    public Dictionary<Guid, List<TeamMember>> MembersByHabit { get; } = new();

    public Task<TeamMember?> GetMemberByIdAsync(Guid memberId)
        => Task.FromResult(ById.TryGetValue(memberId, out var m) ? m : null);

    public Task<List<TeamMember>> GetMembersByHabitEntriesAsync(Guid habitId)
        => Task.FromResult(MembersByHabit.TryGetValue(habitId, out var l) ? l : new());

    public Task<TeamMember?> GetMemberByEmailAsync(string email) => throw new NotImplementedException();
    public Task<TeamMember> CreateMemberAsync(TeamMember member) => throw new NotImplementedException();
    public Task<List<TeamMember>> GetMembersByIdsAsync(List<Guid> memberIds) => throw new NotImplementedException();
    public Task UpdatePasswordAsync(Guid memberId, string newPasswordHash) => throw new NotImplementedException();
    public Task ChangeEmailAsync(Guid memberId, string newEmail) => throw new NotImplementedException();
    public Task<bool> EmailAlreadyExistsAsync(string email) => throw new NotImplementedException();
}

public sealed class FakeReminderRepository : IReminderRepository
{
    public List<Guid> DisabledForHabit { get; } = new();

    public Task DisableAllRemindersForHabitAsync(Guid habitId)
    {
        DisabledForHabit.Add(habitId);
        return Task.CompletedTask;
    }

    public Task<Reminder?> GetReminderByHabitAndMemberAsync(Guid habitId, Guid memberId) => throw new NotImplementedException();
    public Task<List<Reminder>> GetEnabledRemindersWithHabitAndMemberAsync() => throw new NotImplementedException();
    public Task<Reminder> CreateReminderAsync(Reminder reminder) => throw new NotImplementedException();
    public Task CreateMissingRemindersForHabitAsync(Guid habitId, List<Guid> memberIds) => throw new NotImplementedException();
    public Task CreateMissingRemindersForMemberAsync(Guid memberId, List<Guid> habitIds) => throw new NotImplementedException();
    public Task<bool> SetHabitReminderTimeAsync(Guid habitId, TimeOnly reminderTime) => throw new NotImplementedException();
    public Task<bool> ClearHabitReminderTimeAsync(Guid habitId) => throw new NotImplementedException();
    public Task<bool> SetReminderEnabledAsync(Guid habitId, Guid memberId, bool enabled) => throw new NotImplementedException();
    public Task<bool> UpdateLastSentAtAsync(Guid reminderId, DateTime lastSentAt) => throw new NotImplementedException();
}

public sealed class FakeUnitOfWork: IUnitOfWork
{
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await action();
    }
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
    {
        return await action();
    }
}

public sealed class FakeLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel)
        => false;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    { }
}
public static class HabitTestIds
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TeamId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid HabitId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid MemberId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid OtherMemberId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid EntryId = Guid.Parse("66666666-6666-6666-6666-666666666666");
}
