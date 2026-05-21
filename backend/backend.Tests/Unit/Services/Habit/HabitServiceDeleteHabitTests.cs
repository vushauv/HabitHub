using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceDeleteHabitTests : HabitServiceTestBase
{
    [Fact]
    public async Task NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Sut().DeleteHabit(UserId, HabitId));
    }

    [Fact]
    public async Task NotOwner_Forbidden()
    {
        SeedHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().DeleteHabit(UserId, HabitId));
    }

    [Fact]
    public async Task DeleteFalse_NotFound()
    {
        SeedHabit();
        SeedOwnership();
        _habits.DeleteResult = false;

        await Assert.ThrowsAsync<NotFoundException>(() => Sut().DeleteHabit(UserId, HabitId));
    }

    [Fact]
    public async Task Success()
    {
        SeedHabit();
        SeedOwnership();

        await Sut().DeleteHabit(UserId, HabitId);

        Assert.Equal(HabitId, _habits.LastDeletedId);
    }
}
