using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cms.Domain.Entities;

public class UserProfile
{
    [Key] public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Column(TypeName = "varchar(100)")]
    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    [Column(TypeName = "varchar(500)")]
    public string? Bio { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string? Timezone { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
