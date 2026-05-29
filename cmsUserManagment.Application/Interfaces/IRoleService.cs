using cmsUserManagment.Application.DTO;

namespace cmsUserManagment.Application.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleResponse>> GetAllRoles();
    Task<RoleResponse> GetRoleById(Guid id);
    Task<RoleResponse> CreateRole(CreateRoleDto dto);
    Task<RoleResponse> UpdateRole(Guid id, UpdateRoleDto dto);
    Task DeleteRole(Guid id);
    Task<IEnumerable<UserRoleResponse>> GetUserRoles(Guid userId);
    Task<UserRoleResponse> AssignRole(Guid userId, AssignRoleDto dto);
    Task RemoveRole(Guid userId, Guid roleId);
}
