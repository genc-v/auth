using cmsUserManagment.Application.Common;
using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsUserManagment.Controllers;

[Route("api/profile")]
[ApiController]
[Authorize]
public class ProfileController(
    IProfileService profileService,
    JwtDecoder jwtDecoder,
    HeadersManager headersManager) : ControllerBase
{
    private readonly IProfileService _profileService = profileService;
    private readonly JwtDecoder _jwtDecoder = jwtDecoder;
    private readonly HeadersManager _headersManager = headersManager;

    /// <summary>Gets the current user's profile.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile()
    {
        string token = _headersManager.GetJwtFromHeader(Request.Headers);
        Guid userId = _jwtDecoder.GetUserid(token);
        var profile = await _profileService.GetProfile(userId);
        return Ok(profile);
    }

    /// <summary>Gets a user's profile by userId (admin only).</summary>
    [HttpGet("{userId}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfileByUserId(Guid userId)
    {
        var profile = await _profileService.GetProfile(userId);
        return Ok(profile);
    }

    /// <summary>Creates or updates the current user's profile.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertMyProfile([FromBody] UpdateProfileDto dto)
    {
        string token = _headersManager.GetJwtFromHeader(Request.Headers);
        Guid userId = _jwtDecoder.GetUserid(token);
        var profile = await _profileService.UpsertProfile(userId, dto);
        return Ok(profile);
    }
}
