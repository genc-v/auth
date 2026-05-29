using cmsUserManagment.Application.DTO;

namespace cmsUserManagment.Application.Interfaces;

public interface ILogService
{
    Task WriteLog(Guid? userId, string action, string? details = null, string? ipAddress = null);
    Task<PaginatedResult<LogResponse>> GetLogs(Guid? userId, int pageNumber, int pageSize);
}
