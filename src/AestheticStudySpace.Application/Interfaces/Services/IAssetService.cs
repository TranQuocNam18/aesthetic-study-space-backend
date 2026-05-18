using AestheticStudySpace.Application.DTOs.Assets;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IAssetService
{
    Task<IReadOnlyList<AssetDto>> GetAllAsync(string? type, string? category, CancellationToken cancellationToken = default);
    Task<AssetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AssetDto> CreateAsync(CreateAssetRequestDto request, CancellationToken cancellationToken = default);
    Task<AssetDto> UpdateAsync(Guid id, UpdateAssetRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
