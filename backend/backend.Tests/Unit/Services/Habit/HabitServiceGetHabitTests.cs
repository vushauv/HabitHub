using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceGetHabitTests : HabitServiceTestBase
{
    private Models.Habit SeedRunHabit() => SeedHabit(
        type: HabitType.Quantitative,
        name: "Run",
        goal: "5k",
        unit: backend.Enums.Unit.Minutes,
        expiryDate: DateTime.UtcNow.AddDays(7));

    [Fact]
    public async Task NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().GetHabit(UserId, UserType.Creator, HabitId));
    }

    [Fact]
    public async Task Creator_NotOwner_Forbidden()
    {
        SeedRunHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().GetHabit(UserId, UserType.Creator, HabitId));
    }

    [Fact]
    public async Task Member_NotActive_Forbidden()
    {
        SeedRunHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().GetHabit(UserId, UserType.Member, HabitId));
    }

    [Fact]
    public async Task InvalidUserType_AuthRequired()
    {
        SeedRunHabit();

        await Assert.ThrowsAsync<AuthRequiredException>(() =>
            Sut().GetHabit(UserId, (UserType)999, HabitId));
    }

    [Fact]
    public async Task Creator_HappyPath_MapsDto()
    {
        var habit = SeedRunHabit();
        SeedOwnership();

        var dto = await Sut().GetHabit(UserId, UserType.Creator, HabitId);

        Assert.Equal(habit.HabitId, dto.HabitId);
        Assert.Equal(habit.Name, dto.Name);
        Assert.Equal(habit.Goal, dto.Goal);
        Assert.Equal(habit.HabitState, dto.HabitState);
        Assert.Equal(habit.HabitType, dto.HabitType);
        Assert.Equal(habit.Unit, dto.Unit);
        Assert.Equal(habit.ExpiryDate, dto.ExpiryDate);
    }

    [Fact]
    public async Task Member_Active_Ok()
    {
        SeedRunHabit();
        _memberships.Active[(TeamId, UserId)] = true;

        var dto = await Sut().GetHabit(UserId, UserType.Member, HabitId);

        Assert.Equal(HabitId, dto.HabitId);
    }
}
