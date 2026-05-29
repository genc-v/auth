using cms.Domain.Entities;

using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Persistance;

using Microsoft.EntityFrameworkCore;

namespace cmsUserManagment.Infrastructure.Repositories;

public class LogService(AppDbContext dbContext) : ILogService
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task WriteLog(Guid? userId, string action, string? details = null, string? ipAddress = null)
    {
        ActivityLog log = new()
        {
            UserId = userId,
            Action = action,
            Details = details,
            IpAddress = ipAddress
        };

        await _dbContext.ActivityLogs.AddAsync(log);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<PaginatedResult<LogResponse>> GetLogs(Guid? userId, int pageNumber, int pageSize)
    {
        IQueryable<ActivityLog> query = _dbContext.ActivityLogs.AsQueryable();

        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId);

        int total = await query.CountAsync();

        List<LogResponse> items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LogResponse
            {
                Id = l.Id,
                UserId = l.UserId,
                Action = l.Action,
                Details = l.Details,
                IpAddress = l.IpAddress,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResult<LogResponse>
        {
            Items = items,
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
