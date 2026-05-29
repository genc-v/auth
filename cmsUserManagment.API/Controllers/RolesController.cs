using cmsUserManagment.Application.Common;
using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsUserManagment.Controllers;

[Route("api/roles")]
[ApiController]
[Authorize]
public class RolesController(IRoleService roleService, HeadersManager headersManager) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;
    private readonly HeadersManager _headersManager = headersManager;

    /// <summary>Gets all roles.</summary>
    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleService.GetAllRoles();
        return Ok(roles);
    }

    /// <summary>Gets a role by ID.</summary>
    [HttpGet("{id}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        var role = await _roleService.GetRoleById(id);
        return Ok(role);
    }

    /// <summary>Creates a new role.</summary>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        var role = await _roleService.CreateRole(dto);
        return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
    }

    /// <summary>Updates a role.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var role = await _roleService.UpdateRole(id, dto);
        return Ok(role);
    }

    /// <summary>Deletes a role.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        await _roleService.DeleteRole(id);
        return NoContent();
    }

    /// <summary>Gets all roles assigned to a user.</summary>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(IEnumerable<UserRoleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        var roles = await _roleService.GetUserRoles(userId);
        return Ok(roles);
    }

    /// <summary>Assigns a role to a user.</summary>
    [HttpPost("user/{userId}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(UserRoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignRoleDto dto)
    {
        var userRole = await _roleService.AssignRole(userId, dto);
        return CreatedAtAction(nameof(GetUserRoles), new { userId }, userRole);
    }

    /// <summary>Removes a role from a user.</summary>
    [HttpDelete("user/{userId}/{roleId}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId)
    {
        await _roleService.RemoveRole(userId, roleId);
        return NoContent();
    }
}
