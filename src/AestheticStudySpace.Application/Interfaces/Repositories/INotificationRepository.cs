using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountForAdminAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetForAdminAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task BulkMarkAsReadForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
