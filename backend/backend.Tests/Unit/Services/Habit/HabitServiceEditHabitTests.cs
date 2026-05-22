using backend.Dtos.HabitDtos;
using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceEditHabitTests : HabitServiceTestBase
{
    private Models.Habit SetupOwnedActiveHabit()
    {
        var habit = SeedHabit(creatorId: UserId, name: "Old", goal: "old goal", expiryDate: DateTime.UtcNow.AddDays(5));
        SeedOwnership();
        return habit;
    }

    [Fact]
    public async Task ClearGoal_WithGoalSet_Throws()
    {
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto(null, "new", null, ClearGoal: true)));
    }

    [Fact]
    public async Task ClearExpiryDate_WithExpirySet_Throws()
    {
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto(null, null, DateTime.UtcNow.AddDays(2), ClearExpiryDate: true)));
    }

    [Fact]
    public async Task EmptyNameAfterTrim_Throws()
    {
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto("   ", null, null)));
    }

    [Fact]
    public async Task ExpiryDateInPast_Throws()
    {
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto(null, null, DateTime.UtcNow.AddDays(-1))));
    }

    [Fact]
    public async Task HabitMissing_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto("X", null, null)));
    }

    [Fact]
    public async Task NotOwner_Forbidden()
    {
        SeedHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto("X", null, null)));
    }

    [Fact]
    public async Task Archived_Conflict()
    {
        SeedHabit(state: HabitState.Archived);
        SeedOwnership();

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto("X", null, null)));

        Assert.Equal("habit-archived", ex.ErrorCode);
    }

    [Fact]
    public async Task OnlyName_Updates()
    {
        var habit = SetupOwnedActiveHabit();

        var result = await Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto("  New  ", null, null));

        Assert.Equal("New", result.Name);
        Assert.Equal("old goal", result.Goal);
        Assert.Equal(habit.ExpiryDate, result.ExpiryDate);
        Assert.Same(habit, _habits.LastUpdated);
    }

    [Fact]
    public async Task OnlyGoal_Updates()
    {
        SetupOwnedActiveHabit();

        var result = await Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto(null, " new goal ", null));

        Assert.Equal("Old", result.Name);
        Assert.Equal("new goal", result.Goal);
    }

    [Fact]
    public async Task OnlyExpiry_Updates()
    {
        SetupOwnedActiveHabit();
        var newExpiry = DateTime.UtcNow.AddDays(10);

        var result = await Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto(null, null, newExpiry));

        Assert.Equal(newExpiry, result.ExpiryDate);
    }

    [Fact]
    public async Task ClearGoal_SetsNull()
    {
        SetupOwnedActiveHabit();

        var result = await Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto(null, null, null, ClearGoal: true));

        Assert.Null(result.Goal);
    }

    [Fact]
    public async Task ClearExpiryDate_SetsNull()
    {
        SetupOwnedActiveHabit();

        var result = await Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto(null, null, null, ClearExpiryDate: true));

        Assert.Null(result.ExpiryDate);
    }

    [Fact]
    public async Task Update_RepoCalled()
    {
        SetupOwnedActiveHabit();

        await Sut().EditHabit(UserId, HabitId, new EditHabitRequestDto("Z", null, null));

        Assert.NotNull(_habits.LastUpdated);
        Assert.Equal("Z", _habits.LastUpdated!.Name);
    }
}
