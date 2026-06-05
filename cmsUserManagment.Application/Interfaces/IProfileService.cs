using cmsUserManagment.Application.DTO;

namespace cmsUserManagment.Application.Interfaces;

public interface IProfileService
{
    Task<ProfileResponse> GetProfile(Guid userId);
    Task<ProfileResponse> UpsertProfile(Guid userId, UpdateProfileDto dto);
    Task<IEnumerable<PublicUserDto>> GetProfilesByIds(IEnumerable<Guid> ids);
}
