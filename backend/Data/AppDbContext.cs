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
    public DbSet<InviteCode> InviteCodes => Set<InviteCode>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitEntry> HabitEntries => Set<HabitEntry>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<TeamChat> TeamChats => Set<TeamChat>();
    public DbSet<Message> Messages => Set<Message>();

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

            e.HasIndex(s => new { s.UserId, s.UserType });
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
        modelBuilder.Entity<InviteCode>(e =>
        {
           e.HasKey(i => i.CodeId);
           e.Property(i => i.Code).IsRequired().HasMaxLength(64);
           e.Property(i => i.TeamId).IsRequired();
           e.Property(i => i.ExpiryDate).IsRequired();
           e.Property(i => i.Status).IsRequired(); 


           e.HasOne(m => m.Team)
            .WithMany(t => t.InviteCodes)
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade); // what delete?

           e.HasIndex(i => i.Code).IsUnique();
           e.HasIndex(i => i.TeamId);
        });
        modelBuilder.Entity<Habit>(e =>
        {
           e.HasKey(h => h.HabitId);
           e.Property(h => h.TeamId).IsRequired();
           e.Property(h => h.Name).HasMaxLength(256).IsRequired();
           e.Property(h => h.Goal).HasMaxLength(512);
           e.Property(h => h.CreatorId).IsRequired();
           e.Property(h => h.HabitState).IsRequired();
           e.Property(h => h.HabitType).IsRequired();
           e.Property(h => h.Unit).IsRequired(false);
           e.Property(h => h.ExpiryDate).IsRequired(false);
           e.Property(h => h.ReminderTime).IsRequired(false);

           e.HasOne(h => h.Team)
           .WithMany(t => t.Habits)
           .HasForeignKey(h => h.TeamId)
           .OnDelete(DeleteBehavior.Cascade);

           e.HasOne(h => h.Creator)
           .WithMany()
           .HasForeignKey(h => h.CreatorId)
           .OnDelete(DeleteBehavior.Restrict);
           
           e.HasIndex(h => h.TeamId);
        });
        modelBuilder.Entity<HabitEntry>(e =>
        {
            e.HasKey(he => he.EntryId);
            e.Property(he => he.HabitId).IsRequired();
            e.Property(he => he.MemberId).IsRequired();
            e.Property(he => he.Status).IsRequired();
            e.Property(he => he.Notes).HasMaxLength(1000);
            e.Property(he => he.LoggedAt).IsRequired();
            e.Property(he => he.LogDate).IsRequired();

            e.HasOne(he => he.Habit)
            .WithMany(h => h.Entries)
            .HasForeignKey(he => he.HabitId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(he => he.Member)
            .WithMany(m => m.HabitEntries)
            .HasForeignKey(he => he.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
            
            e.HasIndex(he => new { he.HabitId, he.MemberId, he.LogDate })
            .IsUnique();
        });
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.NotificationId);

            e.Property(n => n.UserId).IsRequired();
            e.Property(n => n.UserType).IsRequired();
            e.Property(n => n.Content).IsRequired().HasMaxLength(2000);
            e.Property(n => n.CreatedAt).IsRequired();
            e.Property(n => n.Status).IsRequired();
            e.Property(n => n.Type).IsRequired();

            e.HasIndex(n => new { n.UserId, n.UserType });
            e.HasIndex(n => new { n.UserId, n.UserType, n.Status, n.Type });
        });
        modelBuilder.Entity<Reminder>(e =>
        {
            e.HasKey(r => r.ReminderId);

            e.Property(r => r.MemberId).IsRequired();
            e.Property(r => r.HabitId).IsRequired();
            e.Property(r => r.Enabled).IsRequired();
            e.Property(r => r.LastSentAt).IsRequired(false);

            e.HasOne(r => r.Member)
            .WithMany(m => m.Reminders)
            .HasForeignKey(r => r.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Habit)
            .WithMany(h => h.Reminders)
            .HasForeignKey(r => r.HabitId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(r => new { r.HabitId, r.MemberId }).IsUnique();
        });
        modelBuilder.Entity<TeamChat>(e =>
        {
            e.HasKey(tc => tc.ChatId);

            e.Property(tc => tc.TeamId).IsRequired();

            e.HasOne(tc => tc.Team)
            .WithOne(t => t.Chat)
            .HasForeignKey<TeamChat>(tc => tc.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(e =>
        {
            e.HasKey(m => m.MessageId);

            e.Property(m => m.ChatId).IsRequired();
            e.Property(m => m.UserId).IsRequired();
            e.Property(m => m.UserType).IsRequired();
            e.Property(m => m.Content).IsRequired().HasMaxLength(2000); //TODO tell Kamila
            e.Property(m => m.SendDate).IsRequired();

            e.HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(m => m.ChatId);
        });
    }
}
