using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceGetMyTodayEntryStatusTests : HabitServiceTestBase
{
    [Fact]
    public async Task NonMember_Forbidden()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().GetMyTodayEntryStatus(UserId, UserType.Creator, HabitId));
    }

    [Fact]
    public async Task HabitMissing_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Sut().GetMyTodayEntryStatus(UserId, UserType.Member, HabitId));
    }

    [Fact]
    public async Task MemberMissing_Forbidden()
    {
        SeedHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().GetMyTodayEntryStatus(UserId, UserType.Member, HabitId));
    }

    [Fact]
    public async Task NotActive_Forbidden()
    {
        SeedHabit();
        _members.ById[UserId] = MakeMember(MemberId);

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().GetMyTodayEntryStatus(UserId, UserType.Member, HabitId));
    }

    [Fact]
    public async Task NoEntry_PendingAndNull()
    {
        SeedHabit();
        SeedActiveMembership();

        var dto = await Sut().GetMyTodayEntryStatus(UserId, UserType.Member, HabitId);

        Assert.Equal(EntryStatus.Pending, dto.Status);
        Assert.Null(dto.Entry);
    }

    [Fact]
    public async Task EntryExists_ReturnsStatusAndDto()
    {
        SeedHabit();
        SeedActiveMembership();
        var entry = SeedEntryToday(value: 2);
        entry.Notes = "x";

        var dto = await Sut().GetMyTodayEntryStatus(UserId, UserType.Member, HabitId);

        Assert.Equal(EntryStatus.Logged, dto.Status);
        Assert.NotNull(dto.Entry);
        Assert.Equal(entry.EntryId, dto.Entry!.EntryId);
        Assert.Equal(2, dto.Entry.Value);
    }
}
