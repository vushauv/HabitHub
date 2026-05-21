using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceArchiveHabitTests : HabitServiceTestBase
{
    [Fact]
    public async Task NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Sut().ArchiveHabit(UserId, HabitId));
    }

    [Fact]
    public async Task NotOwner_Forbidden()
    {
        SeedHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().ArchiveHabit(UserId, HabitId));
    }

    [Fact]
    public async Task AlreadyArchived_NoOp()
    {
        SeedHabit(state: HabitState.Archived);
        SeedOwnership();

        await Sut().ArchiveHabit(UserId, HabitId);

        Assert.Null(_habits.LastArchivedId);
        Assert.Empty(_reminders.DisabledForHabit);
    }

    [Fact]
    public async Task ArchiveReturnsFalse_NotFound()
    {
        SeedHabit();
        SeedOwnership();
        _habits.ArchiveResult = false;

        await Assert.ThrowsAsync<NotFoundException>(() => Sut().ArchiveHabit(UserId, HabitId));
        Assert.Empty(_reminders.DisabledForHabit);
    }

    [Fact]
    public async Task Success_DisablesReminders()
    {
        SeedHabit();
        SeedOwnership();

        await Sut().ArchiveHabit(UserId, HabitId);

        Assert.Equal(HabitId, _habits.LastArchivedId);
        Assert.Single(_reminders.DisabledForHabit);
        Assert.Equal(HabitId, _reminders.DisabledForHabit[0]);
    }
}
