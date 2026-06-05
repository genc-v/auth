using cms.Domain.Entities;

using cmsUserManagment.Application.Common.ErrorCodes;
using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Persistance;

using Microsoft.EntityFrameworkCore;

namespace cmsUserManagment.Infrastructure.Repositories;

public class ProfileService(AppDbContext dbContext, ILogService logService) : IProfileService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogService _logService = logService;

    public async Task<ProfileResponse> GetProfile(Guid userId)
    {
        UserProfile? profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) throw GeneralErrorCodes.NotFound;

        return MapToResponse(profile);
    }

    public async Task<ProfileResponse> UpsertProfile(Guid userId, UpdateProfileDto dto)
    {
        UserProfile? profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
                throw GeneralErrorCodes.NotFound;

            profile = new UserProfile { UserId = userId };
            await _dbContext.UserProfiles.AddAsync(profile);
        }

        if (dto.DisplayName != null) profile.DisplayName = dto.DisplayName;
        if (dto.FirstName != null) profile.FirstName = dto.FirstName;
        if (dto.LastName != null) profile.LastName = dto.LastName;
        if (dto.AvatarUrl != null) profile.AvatarUrl = dto.AvatarUrl;
        if (dto.Bio != null) profile.Bio = dto.Bio;
        if (dto.PhoneNumber != null) profile.PhoneNumber = dto.PhoneNumber;
        if (dto.Timezone != null) profile.Timezone = dto.Timezone;
        profile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await _logService.WriteLog(userId, "Profile Updated");

        return MapToResponse(profile);
    }

    public async Task<IEnumerable<PublicUserDto>> GetProfilesByIds(IEnumerable<Guid> ids)
    {
        return await _dbContext.UserProfiles
            .Where(p => ids.Contains(p.UserId))
            .Select(p => new PublicUserDto
            {
                UserId = p.UserId,
                DisplayName = p.DisplayName,
                AvatarUrl = p.AvatarUrl,
                Bio = p.Bio
            })
            .ToListAsync();
    }

    private static ProfileResponse MapToResponse(UserProfile profile) => new()
    {
        UserId = profile.UserId,
        DisplayName = profile.DisplayName,
        FirstName = profile.FirstName,
        LastName = profile.LastName,
        AvatarUrl = profile.AvatarUrl,
        Bio = profile.Bio,
        PhoneNumber = profile.PhoneNumber,
        Timezone = profile.Timezone,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };
}
