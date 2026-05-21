using backend.Enums;
using backend.Service;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

public abstract class HabitServiceTestBase
{
    protected readonly FakeHabitRepository _habits = new();
    protected readonly FakeHabitTeamRepository _habitTeams = new();
    protected readonly FakeMembershipRepository _memberships = new();
    protected readonly FakeHabitEntryRepository _entries = new();
    protected readonly FakeTeamMemberRepository _members = new();
    protected readonly FakeReminderRepository _reminders = new();

    protected HabitService Sut() => new(_habits, _habitTeams, _memberships, _entries, _members, _reminders);

    protected static Models.Habit MakeHabit(
        Guid? habitId = null,
        Guid? teamId = null,
        Guid? creatorId = null,
        HabitState state = HabitState.Active,
        HabitType type = HabitType.Binary,
        string name = "Habit",
        string? goal = null,
        backend.Enums.Unit? unit = null,
        DateTime? expiryDate = null)
        => new()
        {
            HabitId = habitId ?? HabitId,
            TeamId = teamId ?? TeamId,
            CreatorId = creatorId ?? Guid.Empty,
            Name = name,
            Goal = goal,
            HabitState = state,
            HabitType = type,
            Unit = unit,
            ExpiryDate = expiryDate
        };

    protected static Models.HabitEntry MakeEntry(
        Guid? memberId = null,
        EntryStatus status = EntryStatus.Logged,
        float? value = null,
        Guid? habitId = null,
        DateOnly? logDate = null,
        string? notes = null)
        => new()
        {
            EntryId = Guid.NewGuid(),
            HabitId = habitId ?? HabitId,
            MemberId = memberId ?? MemberId,
            Status = status,
            Value = value,
            Notes = notes,
            LogDate = logDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            LoggedAt = DateTime.UtcNow
        };

    protected static Models.TeamMember MakeMember(Guid? memberId = null, string name = "X")
        => new() { MemberId = memberId ?? MemberId, Name = name };

    protected Models.Habit SeedHabit(
        HabitState state = HabitState.Active,
        HabitType type = HabitType.Binary,
        Guid? creatorId = null,
        string name = "Habit",
        string? goal = null,
        backend.Enums.Unit? unit = null,
        DateTime? expiryDate = null)
    {
        var h = MakeHabit(state: state, type: type, creatorId: creatorId, name: name, goal: goal, unit: unit, expiryDate: expiryDate);
        _habits.ById[HabitId] = h;
        return h;
    }

    protected void SeedOwnership() => _habitTeams.Owners[(TeamId, UserId)] = true;

    protected void SeedTeam() =>
        _habitTeams.TeamsById[TeamId] = new Models.HabitTeam { TeamId = TeamId, CreatorId = UserId };

    protected void SeedActiveMembership()
    {
        _members.ById[UserId] = MakeMember(MemberId);
        _memberships.Active[(TeamId, MemberId)] = true;
    }

    protected Models.HabitEntry SeedEntryToday(Guid? entryId = null, Guid? memberId = null, EntryStatus status = EntryStatus.Logged, float? value = null)
    {
        var e = MakeEntry(memberId: memberId, status: status, value: value);
        if (entryId.HasValue) e.EntryId = entryId.Value;
        _entries.Entries.Add(e);
        return e;
    }
}
