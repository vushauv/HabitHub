using backend.Dtos.HabitDtos;
using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceCreateHabitTests : HabitServiceTestBase
{
    private void SetupOwnedTeam()
    {
        SeedTeam();
        SeedOwnership();
    }

    private static CreateHabitRequestDto BinaryRequest(
        string name = "Drink water",
        string? goal = null,
        HabitType? habitType = HabitType.Binary,
        backend.Enums.Unit? unit = null,
        DateTime? expiry = null
    ) => new(name, goal, habitType, unit, expiry);

    [Fact]
    public async Task CreateHabit_TrimsName()
    {
        SetupOwnedTeam();

        var result = await Sut().CreateHabit(UserId, TeamId, BinaryRequest(name: "  Run  "));

        Assert.Equal("Run", result.Name);
        Assert.Equal("Run", _habits.LastCreated!.Name);
    }

    [Fact]
    public async Task CreateHabit_WhitespaceGoal_NormalizedToNull()
    {
        SetupOwnedTeam();

        var result = await Sut().CreateHabit(UserId, TeamId, BinaryRequest(goal: "   "));

        Assert.Null(result.Goal);
        Assert.Null(_habits.LastCreated!.Goal);
    }

    [Fact]
    public async Task CreateHabit_TrimsGoal()
    {
        SetupOwnedTeam();

        var result = await Sut().CreateHabit(UserId, TeamId, BinaryRequest(goal: "  be healthy  "));

        Assert.Equal("be healthy", result.Goal);
    }

    [Fact]
    public async Task CreateHabit_NullHabitType_DefaultsToBinary()
    {
        SetupOwnedTeam();

        var result = await Sut().CreateHabit(UserId, TeamId, BinaryRequest(habitType: null));

        Assert.Equal(HabitType.Binary, result.HabitType);
    }

    [Fact]
    public async Task CreateHabit_BinaryWithUnit_Throws()
    {
        SetupOwnedTeam();

        var ex = await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().CreateHabit(UserId, TeamId, BinaryRequest(unit: backend.Enums.Unit.Minutes))
        );

        Assert.Contains("Unit is allowed only for quantitative", ex.Message);
        Assert.Null(_habits.LastCreated);
    }

    [Fact]
    public async Task CreateHabit_QuantitativeWithoutUnit_Throws()
    {
        SetupOwnedTeam();

        var ex = await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().CreateHabit(UserId, TeamId, BinaryRequest(habitType: HabitType.Quantitative, unit: null))
        );

        Assert.Contains("Unit is required for quantitative", ex.Message);
    }

    [Fact]
    public async Task CreateHabit_ExpiryDateInPast_Throws()
    {
        SetupOwnedTeam();

        var ex = await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().CreateHabit(UserId, TeamId, BinaryRequest(expiry: DateTime.UtcNow.AddDays(-1)))
        );

        Assert.Contains("Expiry date must be in the future", ex.Message);
    }

    [Fact]
    public async Task CreateHabit_ExpiryDateNowOrBefore_Throws()
    {
        SetupOwnedTeam();

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().CreateHabit(UserId, TeamId, BinaryRequest(expiry: DateTime.UtcNow.AddSeconds(-1)))
        );
    }

    [Fact]
    public async Task CreateHabit_TeamNotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().CreateHabit(UserId, TeamId, BinaryRequest())
        );
    }

    [Fact]
    public async Task CreateHabit_NotTeamOwner_ThrowsForbidden()
    {
        _habitTeams.TeamsById[TeamId] = new Models.HabitTeam { TeamId = TeamId, CreatorId = Guid.NewGuid() };

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().CreateHabit(UserId, TeamId, BinaryRequest())
        );

        Assert.Null(_habits.LastCreated);
    }

    [Fact]
    public async Task CreateHabit_HappyPath_PersistsAndReturnsMappedDto()
    {
        SetupOwnedTeam();
        var expiry = DateTime.UtcNow.AddDays(7);

        var req = new CreateHabitRequestDto(
            Name: "Read",
            Goal: "20 pages",
            HabitType: HabitType.Quantitative,
            Unit: backend.Enums.Unit.Minutes,
            ExpiryDate: expiry
        );

        var result = await Sut().CreateHabit(UserId, TeamId, req);

        Assert.NotEqual(Guid.Empty, result.HabitId);
        Assert.Equal(TeamId, result.TeamId);
        Assert.Equal(UserId, result.CreatorId);
        Assert.Equal("Read", result.Name);
        Assert.Equal("20 pages", result.Goal);
        Assert.Equal(HabitState.Active, result.HabitState);
        Assert.Equal(HabitType.Quantitative, result.HabitType);
        Assert.Equal(backend.Enums.Unit.Minutes, result.Unit);
        Assert.Equal(expiry, result.ExpiryDate);

        var saved = _habits.LastCreated!;
        Assert.Equal(result.HabitId, saved.HabitId);
        Assert.Equal(TeamId, saved.TeamId);
        Assert.Equal(UserId, saved.CreatorId);
        Assert.Equal(HabitState.Active, saved.HabitState);
    }
}
