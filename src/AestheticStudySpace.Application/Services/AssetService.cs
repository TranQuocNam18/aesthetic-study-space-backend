using AestheticStudySpace.Application.DTOs.Assets;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Application.Mapping;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssetService(IAssetRepository assetRepository, IUnitOfWork unitOfWork)
    {
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AssetDto>> GetAllAsync(string? type, string? category, CancellationToken cancellationToken = default)
    {
        AssetType? assetType = null;
        AssetCategory? assetCategory = null;

        if (!string.IsNullOrWhiteSpace(type))
            assetType = MappingExtensions.ParseAssetType(type);

        if (!string.IsNullOrWhiteSpace(category))
            assetCategory = MappingExtensions.ParseAssetCategory(category);

        var assets = await _assetRepository.GetAllAsync(assetType, assetCategory, cancellationToken);
        return assets.Select(a => a.ToDto()).ToList();
    }

    public async Task<AssetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Asset '{id}' was not found.");
        return asset.ToDto();
    }

    public async Task<AssetDto> CreateAsync(CreateAssetRequestDto request, CancellationToken cancellationToken = default)
    {
        var asset = new Asset
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            Url = request.Url,
            AssetType = MappingExtensions.ParseAssetType(request.Type),
            Category = MappingExtensions.ParseAssetCategory(request.Category),
            DefaultVolume = Math.Clamp(request.DefaultVolume, 0, 100),
            IsPremium = request.IsPremium,
            PreviewUrl = request.PreviewUrl?.Trim()
        };

        await _assetRepository.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return asset.ToDto();
    }

    public async Task<AssetDto> UpdateAsync(Guid id, UpdateAssetRequestDto request, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Asset '{id}' was not found.");

        asset.Name = request.Name.Trim();
        asset.Description = request.Description;
        asset.Url = request.Url;
        asset.AssetType = MappingExtensions.ParseAssetType(request.Type);
        asset.Category = MappingExtensions.ParseAssetCategory(request.Category);
        asset.DefaultVolume = Math.Clamp(request.DefaultVolume, 0, 100);
        asset.IsPremium = request.IsPremium;
        asset.PreviewUrl = request.PreviewUrl?.Trim();

        await _assetRepository.UpdateAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return asset.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Asset '{id}' was not found.");

        await _assetRepository.DeleteAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
