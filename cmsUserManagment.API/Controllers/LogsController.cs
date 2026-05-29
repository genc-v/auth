using cmsUserManagment.Application.Common;
using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsUserManagment.Controllers;

[Route("api/logs")]
[ApiController]
[Authorize(Roles = AppRoles.Admin)]
public class LogsController(ILogService logService) : ControllerBase
{
    private readonly ILogService _logService = logService;

    /// <summary>Gets activity logs. Optionally filter by userId.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<LogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] Guid? userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var logs = await _logService.GetLogs(userId, pageNumber, pageSize);
        return Ok(logs);
    }
}
