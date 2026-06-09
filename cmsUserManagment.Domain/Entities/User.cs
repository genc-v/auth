using System.ComponentModel.DataAnnotations;

namespace cms.Domain.Entities;

public class User
{
    [Key] public Guid Id { get; set; }

    [Required] [EmailAddress] public required string Email { get; set; }

    public required string Username { get; set; }

    [Required] public required string Password { get; set; }

    public string? TwoFactorSecret { get; set; }

    public bool IsTwoFactorEnabled { get; set; } = false;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    public ICollection<DeviceToken> DeviceTokens { get; set; } = new List<DeviceToken>();
    public UserProfile? Profile { get; set; }
}
