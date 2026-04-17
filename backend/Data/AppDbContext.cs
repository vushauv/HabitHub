using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TeamCreator> TeamCreators => Set<TeamCreator>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<HabitTeam> HabitTeams => Set<HabitTeam>();
    public DbSet<Membership> Memberships => Set<Membership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TeamCreator>(e => 
        {
            e.HasKey(c => c.CreatorId);
            e.Property(c => c.Email).IsRequired().HasMaxLength(256);
            e.HasIndex(c => c.Email).IsUnique();
            e.Property(c => c.PasswordHash).IsRequired();
            e.Property(c => c.Name).IsRequired().HasMaxLength(256);
        });
        modelBuilder.Entity<TeamMember>(e => 
        {
            e.HasKey(m => m.MemberId);
            e.Property(m => m.Email).IsRequired().HasMaxLength(256);
            e.HasIndex(m => m.Email).IsUnique();
            e.Property(m => m.PasswordHash).IsRequired();
            e.Property(m => m.Name).IsRequired().HasMaxLength(256);
            e.Property(m => m.Timezone).IsRequired().HasMaxLength(256);
        });
        modelBuilder.Entity<Session>(e => 
        {
            e.HasKey(s => s.SessionId);
            e.Property(s => s.SessionId).IsRequired().HasMaxLength(64);
            e.Property(s => s.UserId).IsRequired();
            e.Property(s => s.UserType).IsRequired();
            e.Property(s => s.CreatedAt).IsRequired();
            e.Property(s => s.LastActiveAt).IsRequired();
            e.Property(s => s.ExpiresAt).IsRequired();
            e.Property(s => s.SessionState).IsRequired();
        });
        modelBuilder.Entity<HabitTeam>(e =>
        {
           e.HasKey(t => t.TeamId);
           e.Property(t => t.Name).IsRequired().HasMaxLength(256);
           e.Property(t => t.CreatorId).IsRequired();

           e.HasOne(t => t.Creator)
            .WithMany(c => c.Teams)
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

           e.HasIndex(t => t.CreatorId);
        });
        modelBuilder.Entity<Membership>(e =>
        {
            e.HasKey(m => m.MembershipId);
            e.Property(m => m.TeamId).IsRequired();
            e.Property(m => m.MemberId).IsRequired();
            e.Property(m => m.Status).IsRequired();

            e.HasOne(m => m.Team)
            .WithMany(t => t.Memberships)
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Member)
            .WithMany(mem => mem.Memberships)
            .HasForeignKey(m => m.MemberId)
            .OnDelete(DeleteBehavior.Restrict); // we don't specify account deletion

            e.HasIndex(m => new {m.TeamId, m.MemberId}).IsUnique();
        });
    }
}
