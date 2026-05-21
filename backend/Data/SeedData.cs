using backend.Enums;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class SeedData
{
    public static async Task SeedHabitsAsync(AppDbContext db, ILogger logger)
    {
        (string TeamName, (string Name, string? Goal, HabitType Type, Unit? Unit)[])[] teamHabits =
        [
            ("Team Alpha",
            [
                ("Daily Run",    "Run 5km every day",          HabitType.Quantitative, Unit.Km),
                ("Drink Water",  "Drink 8 cups daily",         HabitType.Quantitative, Unit.Cups),
                ("Read 30 Min",  "Read every day",             HabitType.Binary,       null),
            ]),
            ("Team Beta",
            [
                ("Morning Workout", "Complete morning workout", HabitType.Binary,       null),
                ("Track Steps",     "Walk 10,000 steps daily",  HabitType.Quantitative, Unit.Steps),
            ]),
        ];

        int habitCount = 0;
        foreach (var (teamName, habits) in teamHabits)
        {
            HabitTeam? team = await db.HabitTeams.FirstOrDefaultAsync(t => t.Name == teamName);
            if (team == null) continue;

            foreach (var (name, goal, type, unit) in habits)
            {
                bool exists = await db.Habits.AnyAsync(h => h.TeamId == team.TeamId && h.Name == name);
                if (exists) continue;

                db.Habits.Add(new Habit
                {
                    HabitId = Guid.NewGuid(),
                    TeamId = team.TeamId,
                    Name = name,
                    Goal = goal,
                    CreatorId = team.CreatorId,
                    HabitState = HabitState.Active,
                    HabitType = type,
                    Unit = unit,
                });
                habitCount++;
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {HabitCount} habits", habitCount);
    }

    public static async Task SeedHabitEntriesAsync(AppDbContext db, ILogger logger)
    {
        var habits = await db.Habits
            .Include(h => h.Team)
                .ThenInclude(t => t.Memberships)
                    .ThenInclude(m => m.Member)
            .ToListAsync();

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int entryCount = 0;

        foreach (Habit habit in habits)
        {
            List<TeamMember> members = habit.Team.Memberships
                .Where(m => m.Status == MembershipStatus.Active)
                .Select(m => m.Member)
                .ToList();

            foreach (TeamMember member in members)
            {
                int memberSeed = Math.Abs(member.MemberId.GetHashCode());

                for (int daysAgo = 13; daysAgo >= 1; daysAgo--)
                {
                    DateOnly logDate = today.AddDays(-daysAgo);

                    bool exists = await db.HabitEntries.AnyAsync(e =>
                        e.HabitId == habit.HabitId &&
                        e.MemberId == member.MemberId &&
                        e.LogDate == logDate);
                    if (exists) continue;

                    bool skipped = (memberSeed % 5 == 0) && (daysAgo % 5 == 0);
                    EntryStatus status = skipped ? EntryStatus.Skipped : EntryStatus.Logged;

                    float? value = null;
                    if (status == EntryStatus.Logged && habit.HabitType == HabitType.Quantitative)
                    {
                        value = habit.Unit switch
                        {
                            Unit.Km    => 4.0f + memberSeed % 3,
                            Unit.Cups  => 6.0f + memberSeed % 3,
                            Unit.Steps => 7500.0f + (memberSeed % 4) * 500,
                            _          => 1.0f,
                        };
                    }

                    db.HabitEntries.Add(new HabitEntry
                    {
                        EntryId = Guid.NewGuid(),
                        HabitId = habit.HabitId,
                        MemberId = member.MemberId,
                        LogDate = logDate,
                        LoggedAt = new DateTime(logDate.Year, logDate.Month, logDate.Day, 8, 0, 0, DateTimeKind.Utc),
                        Status = status,
                        Value = value,
                    });
                    entryCount++;
                }
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {EntryCount} habit entries", entryCount);
    }


    public static async Task SeedRemindersAsync(AppDbContext db, ILogger logger)
    {
        var habits = await db.Habits
            .Include(h => h.Team)
                .ThenInclude(t => t.Memberships)
            .ToListAsync();

        int reminderCount = 0;
        foreach (Habit habit in habits)
        {
            if (habit.ReminderTime == null)
            {
                habit.ReminderTime = new TimeOnly(8, 0);
            }

            List<Guid> memberIds = habit.Team.Memberships
                .Where(m => m.Status == MembershipStatus.Active)
                .Select(m => m.MemberId)
                .ToList();

            int idx = 0;
            foreach (Guid memberId in memberIds)
            {
                bool exists = await db.Reminders.AnyAsync(r =>
                    r.HabitId == habit.HabitId && r.MemberId == memberId);
                if (exists) { idx++; continue; }

                bool enabled = idx % 2 == 0;
                DateTime? lastSent = idx % 3 == 0
                    ? DateTime.UtcNow.AddDays(-1)
                    : null;

                db.Reminders.Add(new Reminder
                {
                    ReminderId = Guid.NewGuid(),
                    HabitId = habit.HabitId,
                    MemberId = memberId,
                    Enabled = enabled,
                    LastSentAt = lastSent,
                });
                reminderCount++;
                idx++;
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {ReminderCount} reminders", reminderCount);
    }

    public static async Task SeedNotificationsAsync(AppDbContext db, ILogger logger)
    {
        var members = await db.TeamMembers.ToListAsync();
        var creators = await db.TeamCreators.ToListAsync();

        (string Content, NotificationType Type, NotificationStatus Status, int DaysAgo)[] memberTemplates =
        [
            ("Welcome to HabitHub!",                NotificationType.System,   NotificationStatus.Read,   7),
            ("Your team invite was accepted.",      NotificationType.System,   NotificationStatus.Unread, 3),
            ("Time to log Daily Run.",              NotificationType.Reminder, NotificationStatus.Unread, 1),
            ("Don't forget Drink Water today.",     NotificationType.Reminder, NotificationStatus.Unread, 0),
            ("You missed yesterday's Track Steps.", NotificationType.Reminder, NotificationStatus.Read,   2),
        ];

        (string Content, NotificationType Type, NotificationStatus Status, int DaysAgo)[] creatorTemplates =
        [
            ("Welcome, team creator!",                       NotificationType.System,   NotificationStatus.Read,   10),
            ("A new member joined your team.",               NotificationType.System,   NotificationStatus.Unread, 2),
            ("Weekly summary: 3 habits active.",             NotificationType.System,   NotificationStatus.Unread, 0),
            ("Reminder: review pending invite codes.",       NotificationType.Reminder, NotificationStatus.Unread, 1),
        ];

        int notifCount = 0;

        foreach (TeamMember member in members)
        {
            foreach (var (content, type, status, daysAgo) in memberTemplates)
            {
                bool exists = await db.Notifications.AnyAsync(n =>
                    n.UserId == member.MemberId &&
                    n.UserType == UserType.Member &&
                    n.Content == content);
                if (exists) continue;

                db.Notifications.Add(new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = member.MemberId,
                    UserType = UserType.Member,
                    Content = content,
                    Type = type,
                    Status = status,
                    CreatedAt = DateTime.UtcNow.AddDays(-daysAgo),
                });
                notifCount++;
            }
        }

        foreach (TeamCreator creator in creators)
        {
            foreach (var (content, type, status, daysAgo) in creatorTemplates)
            {
                bool exists = await db.Notifications.AnyAsync(n =>
                    n.UserId == creator.CreatorId &&
                    n.UserType == UserType.Creator &&
                    n.Content == content);
                if (exists) continue;

                db.Notifications.Add(new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = creator.CreatorId,
                    UserType = UserType.Creator,
                    Content = content,
                    Type = type,
                    Status = status,
                    CreatedAt = DateTime.UtcNow.AddDays(-daysAgo),
                });
                notifCount++;
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {NotificationCount} notifications", notifCount);
    }

    public static async Task SeedUsersAsync(AppDbContext db, ILogger logger)
    {
        PasswordHasher<object> hasher = new();
        string passwordHash = hasher.HashPassword(null!, "12345678");

        (string Name, string Email)[] creators =
        [
            ("Alice", "alice@g.com"),
            ("Bob", "bob@g.com"),
            ("Carol", "carol@g.com"),
            ("Dave", "dave@g.com"),
            ("Eve", "eve@g.com")
        ];

        (string Name, string Email)[] members =
        [
            ("Alice", "alice@g.com"),
            ("Bob", "bob@g.com"),
            ("Carol", "carol@g.com"),
            ("Dave", "dave@g.com"),
            ("Eve", "eve@g.com")
        ];

        int creatorCount = 0;
        foreach (var (name, email) in creators)
        {
            if (await db.TeamCreators.AnyAsync(c => c.Email == email)) continue;
            db.TeamCreators.Add(new TeamCreator
            {
                CreatorId = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
            });
            creatorCount++;
        }

        int memberCount = 0;
        foreach (var (name, email) in members)
        {
            if (await db.TeamMembers.AnyAsync(m => m.Email == email)) continue;
            db.TeamMembers.Add(new TeamMember
            {
                MemberId = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
                Timezone = "Europe/Warsaw",
            });
            memberCount++;
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {CreatorCount} creators and {MemberCount} members", creatorCount, memberCount);
    }

    public static async Task SeedTeamsAsync(AppDbContext db, ILogger logger)
    {
        (string TeamName, string CreatorEmail, string[] MemberEmails)[] teams =
        [
            ("Team Alpha", "alice@g.com", ["alice@g.com", "carol@g.com", "dave@g.com", "eve@g.com"]),
            ("Team Beta",  "bob@g.com",   ["bob@g.com",   "carol@g.com", "eve@g.com"])
        ];

        foreach (var (teamName, creatorEmail, memberEmails) in teams)
        {
            TeamCreator? creator = await db.TeamCreators.FirstOrDefaultAsync(c => c.Email == creatorEmail);
            if (creator == null) continue;

            HabitTeam? team = await db.HabitTeams.FirstOrDefaultAsync(t => t.Name == teamName && t.CreatorId == creator.CreatorId);
            if (team == null)
            {
                team = new HabitTeam
                {
                    TeamId = Guid.NewGuid(),
                    Name = teamName,
                    CreatorId = creator.CreatorId,
                };
                db.HabitTeams.Add(team);
                await db.SaveChangesAsync();
                logger.LogInformation("Seeded team {TeamId}", team.TeamId);
            }

            bool chatExists = await db.TeamChats.AnyAsync(c => c.TeamId == team.TeamId);
            if (!chatExists)
            {
                db.TeamChats.Add(new TeamChat
                {
                    ChatId = Guid.NewGuid(),
                    TeamId = team.TeamId,
                });
                await db.SaveChangesAsync();
                logger.LogInformation("Seeded chat for team {TeamId}", team.TeamId);
            }

            int membershipCount = 0;
            foreach (string memberEmail in memberEmails)
            {
                TeamMember? member = await db.TeamMembers.FirstOrDefaultAsync(m => m.Email == memberEmail);
                if (member == null) continue;

                bool exists = await db.Memberships.AnyAsync(m => m.TeamId == team.TeamId && m.MemberId == member.MemberId);
                if (exists) continue;

                db.Memberships.Add(new Membership
                {
                    MembershipId = Guid.NewGuid(),
                    TeamId = team.TeamId,
                    MemberId = member.MemberId,
                    Status = MembershipStatus.Active,
                });
                membershipCount++;
            }

            await db.SaveChangesAsync();
            if (membershipCount > 0)
                logger.LogInformation("Seeded {MembershipCount} memberships for team {TeamId}", membershipCount, team.TeamId);
        }
    }
}
