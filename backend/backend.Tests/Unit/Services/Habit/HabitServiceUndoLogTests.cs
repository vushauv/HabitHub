using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceUndoLogTests : HabitServiceTestBase
{
    [Fact]
    public async Task NonMember_Forbidden()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().UndoLog(UserId, UserType.Creator, HabitId, EntryId));
    }

    [Fact]
    public async Task HabitMissing_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Sut().UndoLog(UserId, UserType.Member, HabitId, EntryId));
    }

    [Fact]
    public async Task MemberMissing_Forbidden()
    {
        SeedHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().UndoLog(UserId, UserType.Member, HabitId, EntryId));
    }

    [Fact]
    public async Task NotActive_Forbidden()
    {
        SeedHabit();
        _members.ById[UserId] = MakeMember(MemberId);

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().UndoLog(UserId, UserType.Member, HabitId, EntryId));
    }

    [Fact]
    public async Task Archived_Conflict()
    {
        SeedHabit(state: HabitState.Archived);
        SeedActiveMembership();

        var ex = await Assert.ThrowsAsync<ConflictException>(() => Sut().UndoLog(UserId, UserType.Member, HabitId, EntryId));
        Assert.Equal("habit-archived", ex.ErrorCode);
    }

    [Fact]
    public async Task NoEntryToday_NotFound()
    {
        SeedHabit();
        SeedActiveMembership();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Sut().UndoLog(UserId, UserType.Member, HabitId, EntryId));
        Assert.Equal("log-not-found", ex.ErrorCode);
    }

    [Fact]
    public async Task EntryIdMismatch_NotFound()
    {
        SeedHabit();
        SeedActiveMembership();
        SeedEntryToday(entryId: Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Sut().UndoLog(UserId, UserType.Member, HabitId, EntryId));
        Assert.Equal("log-not-found", ex.ErrorCode);
    }

    [Fact]
    public async Task DeleteFalse_NotFound()
    {
        SeedHabit();
        SeedActiveMembership();
        SeedEntryToday(entryId: EntryId);
        _entries.DeleteResult = false;

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Sut().UndoLog(UserId, UserType.Member, HabitId, EntryId));
        Assert.Equal("log-not-found", ex.ErrorCode);
    }

    [Fact]
    public async Task Success()
    {
        SeedHabit();
        SeedActiveMembership();
        SeedEntryToday(entryId: EntryId);

        await Sut().UndoLog(UserId, UserType.Member, HabitId, EntryId);

        Assert.Equal(EntryId, _entries.LastDeletedId);
        Assert.Empty(_entries.Entries);
    }
}
