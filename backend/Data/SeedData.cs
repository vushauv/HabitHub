using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class SeedData
{
    public static async Task SeedUsersAsync(AppDbContext db)
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
        }

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
        }

        await db.SaveChangesAsync();
    }
}
