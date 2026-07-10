using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Notification;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateNotificationAsync(Guid? userId, bool isForAdmin, string title, string message, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            IsForAdmin = isForAdmin,
            Title = title.Trim(),
            Message = message.Trim(),
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _notificationRepository.CountForUserAsync(userId, cancellationToken);
        var items = await _notificationRepository.GetForUserAsync(userId, page, pageSize, cancellationToken);

        return new PagedResult<NotificationDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<PagedResult<NotificationDto>> GetAdminNotificationsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _notificationRepository.CountForAdminAsync(cancellationToken);
        var items = await _notificationRepository.GetForAdminAsync(page, pageSize, cancellationToken);

        return new PagedResult<NotificationDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notif = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken)
            ?? throw new NotFoundException("Notification not found.");

        if (!notif.IsForAdmin && notif.UserId != userId)
            throw new ForbiddenException("You do not have permission to modify this notification.");

        notif.IsRead = true;
        await _notificationRepository.UpdateAsync(notif, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.BulkMarkAsReadForUserAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto ToDto(Notification x) =>
        new(x.Id, x.UserId, x.IsForAdmin, x.Title, x.Message, x.IsRead, x.CreatedAt);
}
