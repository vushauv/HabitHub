using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceViewProgressTests : HabitServiceTestBase
{
    private static readonly Guid OtherId = OtherMemberId;

    [Fact]
    public async Task NonMember_Forbidden()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().ViewProgress(UserId, UserType.Creator, HabitId, null));
    }

    [Fact]
    public async Task HabitMissing_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Sut().ViewProgress(UserId, UserType.Member, HabitId, null));
    }

    [Fact]
    public async Task MemberMissing_Forbidden()
    {
        SeedHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().ViewProgress(UserId, UserType.Member, HabitId, null));
    }

    [Fact]
    public async Task NotActive_Forbidden()
    {
        SeedHabit();
        _members.ById[UserId] = MakeMember(MemberId);

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().ViewProgress(UserId, UserType.Member, HabitId, null));
    }

    [Fact]
    public async Task TargetMemberNotActive_NotFound()
    {
        SeedHabit();
        SeedActiveMembership();

        await Assert.ThrowsAsync<NotFoundException>(() => Sut().ViewProgress(UserId, UserType.Member, HabitId, OtherId));
    }

    [Fact]
    public async Task TargetMemberActive_FetchByTarget()
    {
        SeedHabit();
        SeedActiveMembership();
        _memberships.Active[(TeamId, OtherId)] = true;
        _entries.Entries.Add(MakeEntry(memberId: OtherId));
        _entries.Entries.Add(MakeEntry(memberId: MemberId));

        var result = await Sut().ViewProgress(UserId, UserType.Member, HabitId, OtherId);

        Assert.Single(result);
        Assert.Equal(OtherId, result[0].MemberId);
    }

    [Fact]
    public async Task NullTarget_FetchBySelf()
    {
        SeedHabit();
        SeedActiveMembership();
        _entries.Entries.Add(MakeEntry(memberId: MemberId));
        _entries.Entries.Add(MakeEntry(memberId: OtherId));

        var result = await Sut().ViewProgress(UserId, UserType.Member, HabitId, null);

        Assert.Single(result);
        Assert.Equal(MemberId, result[0].MemberId);
    }

    [Fact]
    public async Task MapsDtoList()
    {
        SeedHabit();
        SeedActiveMembership();
        var e = MakeEntry(memberId: MemberId, value: 4, notes: "n");
        _entries.Entries.Add(e);

        var result = await Sut().ViewProgress(UserId, UserType.Member, HabitId, null);

        Assert.Single(result);
        Assert.Equal(e.EntryId, result[0].EntryId);
        Assert.Equal(e.Status, result[0].Status);
        Assert.Equal(e.Value, result[0].Value);
        Assert.Equal(e.Notes, result[0].Notes);
    }
}
