namespace cmsUserManagment.Application.DTO;

public class AssignRoleDto
{
    public required Guid RoleId { get; set; }
}

public class UserRoleResponse
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
