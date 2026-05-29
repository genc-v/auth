using cms.Domain.Entities;

using cmsUserManagment.Application.Common.ErrorCodes;
using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Persistance;

using Microsoft.EntityFrameworkCore;

namespace cmsUserManagment.Infrastructure.Repositories;

public class RoleService(AppDbContext dbContext, ILogService logService) : IRoleService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogService _logService = logService;

    public async Task<IEnumerable<RoleResponse>> GetAllRoles()
    {
        return await _dbContext.Roles
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<RoleResponse> GetRoleById(Guid id)
    {
        Role? role = await _dbContext.Roles.FindAsync(id);
        if (role == null) throw GeneralErrorCodes.NotFound;

        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };
    }

    public async Task<RoleResponse> CreateRole(CreateRoleDto dto)
    {
        if (await _dbContext.Roles.AnyAsync(r => r.Name == dto.Name))
            throw GeneralErrorCodes.Conflict;

        Role role = new() { Name = dto.Name, Description = dto.Description };
        await _dbContext.Roles.AddAsync(role);
        await _dbContext.SaveChangesAsync();
        await _logService.WriteLog(null, "Role Created", $"Role: {role.Name}");

        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };
    }

    public async Task<RoleResponse> UpdateRole(Guid id, UpdateRoleDto dto)
    {
        Role? role = await _dbContext.Roles.FindAsync(id);
        if (role == null) throw GeneralErrorCodes.NotFound;

        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != role.Name)
        {
            if (await _dbContext.Roles.AnyAsync(r => r.Name == dto.Name && r.Id != id))
                throw GeneralErrorCodes.Conflict;
            role.Name = dto.Name;
        }

        if (dto.Description != null) role.Description = dto.Description;

        await _dbContext.SaveChangesAsync();
        await _logService.WriteLog(null, "Role Updated", $"Role: {role.Name}");

        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };
    }

    public async Task DeleteRole(Guid id)
    {
        Role? role = await _dbContext.Roles.FindAsync(id);
        if (role == null) throw GeneralErrorCodes.NotFound;

        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync();
        await _logService.WriteLog(null, "Role Deleted", $"RoleId: {id}");
    }

    public async Task<IEnumerable<UserRoleResponse>> GetUserRoles(Guid userId)
    {
        return await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => new UserRoleResponse
            {
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name,
                AssignedAt = ur.AssignedAt
            })
            .ToListAsync();
    }

    public async Task<UserRoleResponse> AssignRole(Guid userId, AssignRoleDto dto)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
            throw GeneralErrorCodes.NotFound;

        if (!await _dbContext.Roles.AnyAsync(r => r.Id == dto.RoleId))
            throw GeneralErrorCodes.NotFound;

        if (await _dbContext.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == dto.RoleId))
            throw GeneralErrorCodes.Conflict;

        UserRole userRole = new() { UserId = userId, RoleId = dto.RoleId };
        await _dbContext.UserRoles.AddAsync(userRole);
        await _dbContext.SaveChangesAsync();

        await _dbContext.Entry(userRole).Reference(ur => ur.Role).LoadAsync();
        await _logService.WriteLog(userId, "Role Assigned", $"Role: {userRole.Role.Name}");

        return new UserRoleResponse
        {
            UserId = userRole.UserId,
            RoleId = userRole.RoleId,
            RoleName = userRole.Role.Name,
            AssignedAt = userRole.AssignedAt
        };
    }

    public async Task RemoveRole(Guid userId, Guid roleId)
    {
        UserRole? userRole = await _dbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole == null) throw GeneralErrorCodes.NotFound;

        _dbContext.UserRoles.Remove(userRole);
        await _dbContext.SaveChangesAsync();
        await _logService.WriteLog(userId, "Role Removed", $"RoleId: {roleId}");
    }
}
