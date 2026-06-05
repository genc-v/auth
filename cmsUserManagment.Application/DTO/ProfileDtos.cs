namespace cmsUserManagment.Application.DTO;

public class PublicUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Timezone { get; set; }
}

public class ProfileResponse
{
    public Guid UserId { get; set; }
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Timezone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
