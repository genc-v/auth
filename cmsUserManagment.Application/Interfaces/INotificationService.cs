using cmsUserManagment.Application.DTO;

namespace cmsUserManagment.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> GetUserNotifications(Guid userId);
    Task<NotificationResponse> CreateNotification(CreateNotificationDto dto);
    Task MarkAsRead(Guid notificationId, Guid userId);
    Task MarkAllAsRead(Guid userId);
    Task DeleteNotification(Guid notificationId, Guid userId);
}
