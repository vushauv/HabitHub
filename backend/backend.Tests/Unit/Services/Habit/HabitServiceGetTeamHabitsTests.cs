using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceGetTeamHabitsTests : HabitServiceTestBase
{
    [Fact]
    public async Task TeamMissing_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().GetTeamHabits(UserId, UserType.Creator, TeamId, HabitState.Active));
    }

    [Fact]
    public async Task Creator_NotOwner_Forbidden()
    {
        SeedTeam();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().GetTeamHabits(UserId, UserType.Creator, TeamId, HabitState.Active));
    }

    [Fact]
    public async Task Member_NotExist_Forbidden()
    {
        SeedTeam();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().GetTeamHabits(MemberId, UserType.Member, TeamId, HabitState.Active));
    }

    [Fact]
    public async Task Member_NotActive_Forbidden()
    {
        SeedTeam();
        _members.ById[MemberId] = MakeMember(MemberId);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().GetTeamHabits(MemberId, UserType.Member, TeamId, HabitState.Active));
    }

    [Fact]
    public async Task InvalidUserType_AuthRequired()
    {
        SeedTeam();

        await Assert.ThrowsAsync<AuthRequiredException>(() =>
            Sut().GetTeamHabits(UserId, (UserType)999, TeamId, HabitState.Active));
    }

    [Fact]
    public async Task ActiveState_UsesActiveRepo()
    {
        SeedTeam();
        SeedOwnership();
        _habits.ActiveByTeam[TeamId] = new() { MakeHabit(habitId: Guid.NewGuid(), name: "A") };

        var result = await Sut().GetTeamHabits(UserId, UserType.Creator, TeamId, HabitState.Active);

        Assert.Single(result);
        Assert.Equal("A", result[0].Name);
    }

    [Fact]
    public async Task ArchivedState_UsesArchivedRepo()
    {
        SeedTeam();
        SeedOwnership();
        _habits.ArchivedByTeam[TeamId] = new() { MakeHabit(habitId: Guid.NewGuid(), name: "B", state: HabitState.Archived) };

        var result = await Sut().GetTeamHabits(UserId, UserType.Creator, TeamId, HabitState.Archived);

        Assert.Single(result);
        Assert.Equal("B", result[0].Name);
    }

    [Fact]
    public async Task OtherState_Throws()
    {
        SeedTeam();
        SeedOwnership();

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().GetTeamHabits(UserId, UserType.Creator, TeamId, (HabitState)42));
    }

    [Fact]
    public async Task Member_Active_ReturnsList()
    {
        SeedTeam();
        _members.ById[MemberId] = MakeMember(MemberId);
        _memberships.Active[(TeamId, MemberId)] = true;
        _habits.ActiveByTeam[TeamId] = new()
        {
            MakeHabit(habitId: Guid.NewGuid(), name: "X"),
            MakeHabit(habitId: Guid.NewGuid(), name: "Y")
        };

        var result = await Sut().GetTeamHabits(MemberId, UserType.Member, TeamId, HabitState.Active);

        Assert.Equal(2, result.Count);
    }
}
