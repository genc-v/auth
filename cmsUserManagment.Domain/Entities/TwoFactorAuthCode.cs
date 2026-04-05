using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cms.Domain.Entities;

public class TwoFactorAuthCode
{
    [Key] public Guid Id { get; set; }

    public DateTime Expires { get; set; } = DateTime.UtcNow.AddMinutes(10);

    [ForeignKey(nameof(User))] public required Guid UserId { get; set; }

    public User User { get; set; }
}
