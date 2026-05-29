namespace cmsUserManagment.Application.DTO;

public class CreateNotificationDto
{
    public required Guid UserId { get; set; }
    public required string Message { get; set; }
    public string? Type { get; set; }
}

public class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
