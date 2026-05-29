using System.ComponentModel.DataAnnotations;

namespace cms.Domain.Entities;

public class ActivityLog
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    [Required] public required string Action { get; set; }

    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
