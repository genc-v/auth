using cms.Domain.Entities;

using cmsUserManagment.Application.Common.ErrorCodes;
using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Persistance;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace cmsUserManagment.Infrastructure.Repositories;

public class NotificationService(AppDbContext dbContext, IHubContext<NotificationHub> hubContext) : INotificationService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;

    public async Task<IEnumerable<NotificationResponse>> GetUserNotifications(Guid userId)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => MapToResponse(n))
            .ToListAsync();
    }

    public async Task<NotificationResponse> CreateNotification(CreateNotificationDto dto)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == dto.UserId))
            throw GeneralErrorCodes.NotFound;

        Notification notification = new()
        {
            UserId = dto.UserId,
            Message = dto.Message,
            Type = dto.Type
        };

        await _dbContext.Notifications.AddAsync(notification);
        await _dbContext.SaveChangesAsync();

        NotificationResponse response = MapToResponse(notification);

        await _hubContext.Clients
            .Group(dto.UserId.ToString())
            .SendAsync("ReceiveNotification", response);

        return response;
    }

    public async Task MarkAsRead(Guid notificationId, Guid userId)
    {
        Notification? notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) throw GeneralErrorCodes.NotFound;

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkAllAsRead(Guid userId)
    {
        List<Notification> notifications = await _dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (Notification n in notifications)
            n.IsRead = true;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteNotification(Guid notificationId, Guid userId)
    {
        Notification? notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) throw GeneralErrorCodes.NotFound;

        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync();
    }

    private static NotificationResponse MapToResponse(Notification n) => new()
    {
        Id = n.Id,
        UserId = n.UserId,
        Message = n.Message,
        Type = n.Type,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}
