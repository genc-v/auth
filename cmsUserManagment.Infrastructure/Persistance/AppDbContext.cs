using cms.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace cmsUserManagment.Infrastructure.Persistance;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<TwoFactorAuthCode> TwoFactorAuthCodes { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }
    public virtual DbSet<UserProfile> UserProfiles { get; set; }
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany(u => u.ActivityLogs)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Profile)
                  .HasForeignKey<UserProfile>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany(u => u.PasswordResetTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
