using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IAssetRepository
{
    Task<IReadOnlyList<Asset>> GetAllAsync(
        AssetType? type = null,
        string? category = null,
        CancellationToken cancellationToken = default);

    Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Asset asset, CancellationToken cancellationToken = default);
    Task UpdateAsync(Asset asset, CancellationToken cancellationToken = default);
    Task DeleteAsync(Asset asset, CancellationToken cancellationToken = default);
}
