using backend.Enums;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class SeedData
{
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
