namespace cmsUserManagment.Application.DTO;

public class UpdateUserDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public bool IsAdmin { get; set; }
}
