using cmsUserManagment.Application.DTO;

namespace cmsUserManagment.Application.Interfaces;

public interface INotificationService
{
    Task<PaginatedResult<NotificationResponse>> GetUserNotifications(Guid userId, int pageNumber, int pageSize);
    Task<NotificationResponse> CreateNotification(CreateNotificationDto dto);
    Task MarkAsRead(Guid notificationId, Guid userId);
    Task MarkAllAsRead(Guid userId);
    Task DeleteNotification(Guid notificationId, Guid userId);
}
