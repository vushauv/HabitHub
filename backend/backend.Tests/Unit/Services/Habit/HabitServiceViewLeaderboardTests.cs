using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceViewLeaderboardTests : HabitServiceTestBase
{
    private static readonly Guid MemberA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid MemberB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid MemberC = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public async Task HabitMissing_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Sut().ViewLeaderboard(UserId, UserType.Creator, HabitId));
    }

    [Fact]
    public async Task Creator_NotOwner_Forbidden()
    {
        SeedHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().ViewLeaderboard(UserId, UserType.Creator, HabitId));
    }

    [Fact]
    public async Task Member_Missing_Forbidden()
    {
        SeedHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().ViewLeaderboard(UserId, UserType.Member, HabitId));
    }

    [Fact]
    public async Task Member_NotActive_Forbidden()
    {
        SeedHabit();
        _members.ById[UserId] = MakeMember(MemberA);

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().ViewLeaderboard(UserId, UserType.Member, HabitId));
    }

    [Fact]
    public async Task InvalidUserType_AuthRequired()
    {
        SeedHabit();

        await Assert.ThrowsAsync<AuthRequiredException>(() => Sut().ViewLeaderboard(UserId, (UserType)999, HabitId));
    }

    [Fact]
    public async Task Binary_ExcludesSkippedFromTotal()
    {
        SeedHabit();
        SeedOwnership();
        _entries.Entries.AddRange(new[]
        {
            MakeEntry(MemberA),
            MakeEntry(MemberA),
            MakeEntry(MemberA, EntryStatus.Skipped),
            MakeEntry(MemberB),
        });
        _members.MembersByHabit[HabitId] = new()
        {
            MakeMember(MemberA, "Alice"),
            MakeMember(MemberB, "Bob")
        };

        var result = await Sut().ViewLeaderboard(UserId, UserType.Creator, HabitId);

        Assert.Equal(2, result.Count);
        Assert.Equal(MemberA, result[0].MemberId);
        Assert.Equal(2, result[0].LoggedCount);
        Assert.Equal(2, result[0].TotalValue);
        Assert.Equal(1, result[0].Rank);
        Assert.Equal(2, result[1].Rank);
    }

    [Fact]
    public async Task Quantitative_SumsValues_IgnoresNullsAndSkipped()
    {
        SeedHabit(type: HabitType.Quantitative);
        SeedOwnership();
        _entries.Entries.AddRange(new[]
        {
            MakeEntry(MemberA, value: 5),
            MakeEntry(MemberA, value: 10),
            MakeEntry(MemberA),
            MakeEntry(MemberA, EntryStatus.Skipped),
            MakeEntry(MemberB, value: 20),
        });
        _members.MembersByHabit[HabitId] = new()
        {
            MakeMember(MemberA, "Alice"),
            MakeMember(MemberB, "Bob")
        };

        var result = await Sut().ViewLeaderboard(UserId, UserType.Creator, HabitId);

        Assert.Equal(MemberB, result[0].MemberId);
        Assert.Equal(20, result[0].TotalValue);
        Assert.Equal(MemberA, result[1].MemberId);
        Assert.Equal(15, result[1].TotalValue);
    }

    [Fact]
    public async Task Tiebreak_ByLoggedCountDesc()
    {
        SeedHabit(type: HabitType.Quantitative);
        SeedOwnership();
        _entries.Entries.AddRange(new[]
        {
            MakeEntry(MemberA, value: 10),
            MakeEntry(MemberB, value: 5),
            MakeEntry(MemberB, value: 5),
        });
        _members.MembersByHabit[HabitId] = new()
        {
            MakeMember(MemberA, "Alice"),
            MakeMember(MemberB, "Bob")
        };

        var result = await Sut().ViewLeaderboard(UserId, UserType.Creator, HabitId);

        Assert.Equal(MemberB, result[0].MemberId);
        Assert.Equal(2, result[0].LoggedCount);
        Assert.Equal(MemberA, result[1].MemberId);
    }

    [Fact]
    public async Task RanksAreOneBased()
    {
        SeedHabit();
        SeedOwnership();
        _entries.Entries.AddRange(new[]
        {
            MakeEntry(MemberA),
            MakeEntry(MemberB),
            MakeEntry(MemberB),
            MakeEntry(MemberC),
            MakeEntry(MemberC),
            MakeEntry(MemberC),
        });
        _members.MembersByHabit[HabitId] = new()
        {
            MakeMember(MemberA, "A"),
            MakeMember(MemberB, "B"),
            MakeMember(MemberC, "C")
        };

        var result = await Sut().ViewLeaderboard(UserId, UserType.Creator, HabitId);

        Assert.Equal(1, result[0].Rank);
        Assert.Equal(2, result[1].Rank);
        Assert.Equal(3, result[2].Rank);
    }

    [Fact]
    public async Task UnknownMember_FallbackName()
    {
        SeedHabit();
        SeedOwnership();
        _entries.Entries.Add(MakeEntry(MemberA));

        var result = await Sut().ViewLeaderboard(UserId, UserType.Creator, HabitId);

        Assert.Equal("Unknown", result[0].MemberName);
    }
}
