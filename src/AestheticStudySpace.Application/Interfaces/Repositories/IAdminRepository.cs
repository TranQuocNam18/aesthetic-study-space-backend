using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IAdminRepository
{
    Task<(IReadOnlyList<User> users, int total)> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

