using backend.Dtos.HabitEntryDtos;
using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Habit.HabitTestIds;

namespace backend.Tests.Unit.Services.Habit;

[Trait("Category", "Unit")]
public class HabitServiceLogProgressTests : HabitServiceTestBase
{
    [Fact]
    public async Task NonMember_Forbidden()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().LogProgress(UserId, UserType.Creator, HabitId, new LogProgressRequestDto(null, null, EntryStatus.Logged)));
    }

    [Fact]
    public async Task InvalidStatus_Throws()
    {
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(null, null, (EntryStatus)999)));
    }

    [Fact]
    public async Task SkippedWithValue_Throws()
    {
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(5, null, EntryStatus.Skipped)));
    }

    [Fact]
    public async Task HabitMissing_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(null, null, EntryStatus.Logged)));
    }

    [Fact]
    public async Task MemberMissing_Forbidden()
    {
        SeedHabit();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(null, null, EntryStatus.Logged)));
    }

    [Fact]
    public async Task NotActiveMember_Forbidden()
    {
        SeedHabit();
        _members.ById[UserId] = MakeMember(MemberId);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(null, null, EntryStatus.Logged)));
    }

    [Fact]
    public async Task Archived_Conflict()
    {
        SeedHabit(state: HabitState.Archived);
        SeedActiveMembership();

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(null, null, EntryStatus.Logged)));

        Assert.Equal("habit-archived", ex.ErrorCode);
    }

    [Fact]
    public async Task LoggedBinaryWithValue_Throws()
    {
        SeedHabit(type: HabitType.Binary);
        SeedActiveMembership();

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(3, null, EntryStatus.Logged)));
    }

    [Fact]
    public async Task LoggedQuantitativeWithoutValue_Throws()
    {
        SeedHabit(type: HabitType.Quantitative);
        SeedActiveMembership();

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(null, null, EntryStatus.Logged)));
    }

    [Fact]
    public async Task ExistingEntryToday_Conflict()
    {
        SeedHabit(type: HabitType.Binary);
        SeedActiveMembership();
        SeedEntryToday();

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(null, null, EntryStatus.Logged)));

        Assert.Equal("log-already-exists", ex.ErrorCode);
    }

    [Fact]
    public async Task HappyPath_Binary()
    {
        SeedHabit(type: HabitType.Binary);
        SeedActiveMembership();

        var dto = await Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(null, "  note  ", EntryStatus.Logged));

        Assert.Equal(HabitId, dto.HabitId);
        Assert.Equal(MemberId, dto.MemberId);
        Assert.Equal(EntryStatus.Logged, dto.Status);
        Assert.Null(dto.Value);
        Assert.Equal("note", dto.Notes);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), dto.LogDate);
        Assert.NotNull(_entries.LastCreated);
    }

    [Fact]
    public async Task HappyPath_Quantitative()
    {
        SeedHabit(type: HabitType.Quantitative);
        SeedActiveMembership();

        var dto = await Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(7.5f, null, EntryStatus.Logged));

        Assert.Equal(7.5f, dto.Value);
        Assert.Equal(EntryStatus.Logged, dto.Status);
    }

    [Fact]
    public async Task Skipped_ValueNulled()
    {
        SeedHabit(type: HabitType.Quantitative);
        SeedActiveMembership();

        var dto = await Sut().LogProgress(UserId, UserType.Member, HabitId, new LogProgressRequestDto(null, null, EntryStatus.Skipped));

        Assert.Null(dto.Value);
        Assert.Equal(EntryStatus.Skipped, dto.Status);
    }
}
