using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Notification;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(Guid? userId, bool isForAdmin, string title, string message, CancellationToken cancellationToken = default);
    Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<NotificationDto>> GetAdminNotificationsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
