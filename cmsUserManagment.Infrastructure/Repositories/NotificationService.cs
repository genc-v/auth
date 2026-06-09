using cms.Domain.Entities;

using cmsUserManagment.Application.Common.ErrorCodes;
using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Persistance;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace cmsUserManagment.Infrastructure.Repositories;

public class NotificationService(
    AppDbContext dbContext,
    IHubContext<NotificationHub> hubContext,
    IExpoPushService expoPushService) : INotificationService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly IExpoPushService _expoPushService = expoPushService;

    public async Task<PaginatedResult<NotificationResponse>> GetUserNotifications(Guid userId, int pageNumber, int pageSize)
    {
        var query = _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        int totalCount = await query.CountAsync();

        List<NotificationResponse> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => MapToResponse(n))
            .ToListAsync();

        return new PaginatedResult<NotificationResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
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

        List<string> deviceTokens = await _dbContext.DeviceTokens
            .Where(d => d.UserId == dto.UserId)
            .Select(d => d.Token)
            .ToListAsync();

        if (deviceTokens.Count > 0)
        {
            string title = dto.Type switch
            {
                "Login" => "New login to your account",
                _ => "Notification"
            };

            await _expoPushService.SendAsync(
                deviceTokens,
                title,
                dto.Message,
                new { type = dto.Type, notificationId = response.Id });
        }

        return response;
    }

    public async Task RegisterDeviceToken(Guid userId, string token, string? platform)
    {
        if (string.IsNullOrWhiteSpace(token)) throw GeneralErrorCodes.InvalidInput;

        DeviceToken? existing = await _dbContext.DeviceTokens
            .FirstOrDefaultAsync(d => d.Token == token);

        if (existing != null)
        {
            if (existing.UserId != userId)
            {
                existing.UserId = userId;
                existing.Platform = platform;
                await _dbContext.SaveChangesAsync();
            }
            return;
        }

        await _dbContext.DeviceTokens.AddAsync(new DeviceToken
        {
            UserId = userId,
            Token = token,
            Platform = platform
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveDeviceToken(string token)
    {
        DeviceToken? existing = await _dbContext.DeviceTokens
            .FirstOrDefaultAsync(d => d.Token == token);

        if (existing != null)
        {
            _dbContext.DeviceTokens.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }
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
