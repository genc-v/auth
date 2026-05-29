using System.ComponentModel.DataAnnotations;

namespace cms.Domain.Entities;

public class Notification
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required] public required string Message { get; set; }

    public string? Type { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
