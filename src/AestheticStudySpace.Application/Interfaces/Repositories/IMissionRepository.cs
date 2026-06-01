using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IMissionRepository
{
    Task<IReadOnlyList<Mission>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Mission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Mission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Mission>> GetActiveByTriggerKeyAsync(string triggerKey, CancellationToken cancellationToken = default);
    Task<int> CountAllAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Mission>> GetAllAsync(bool includeInactive, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Mission mission, CancellationToken cancellationToken = default);
    Task UpdateAsync(Mission mission, CancellationToken cancellationToken = default);
}

