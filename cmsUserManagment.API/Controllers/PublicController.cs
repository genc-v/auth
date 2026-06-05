using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsUserManagment.Controllers;

[Route("api/public")]
[ApiController]
[AllowAnonymous]
public class PublicController(IUserManagementService userManagementService, IProfileService profileService) : ControllerBase
{
    private readonly IUserManagementService _userManagementService = userManagementService;
    private readonly IProfileService _profileService = profileService;

    /// <summary>
    /// Gets a user by email address. Returns the user or 404.
    /// </summary>
    /// <param name="email">The email address to look up.</param>
    /// <returns>The user if found.</returns>
    [HttpGet("user/by-email")]
    [ProducesResponseType(typeof(PublicUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        var user = await _userManagementService.GetUserByEmail(email);
        return Ok(user);
    }

    /// <summary>
    /// Gets profiles for a list of user IDs.
    /// </summary>
    /// <param name="ids">Array of user GUIDs.</param>
    /// <returns>List of profiles for the given IDs.</returns>
    [HttpPost("users/profiles")]
    [ProducesResponseType(typeof(IEnumerable<ProfileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfilesByIds([FromBody] IEnumerable<Guid> ids)
    {
        var profiles = await _profileService.GetProfilesByIds(ids);
        return Ok(profiles);
    }
}
