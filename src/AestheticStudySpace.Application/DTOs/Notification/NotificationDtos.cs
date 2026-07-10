namespace AestheticStudySpace.Application.DTOs.Notification;

public record NotificationDto(
    Guid Id,
    Guid? UserId,
    bool IsForAdmin,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt);
