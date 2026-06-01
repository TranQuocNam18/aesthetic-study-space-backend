using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IMissionRepository
{
    Task<IReadOnlyList<Mission>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Mission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Mission>> GetActiveByTriggerKeyAsync(string triggerKey, CancellationToken cancellationToken = default);
}

