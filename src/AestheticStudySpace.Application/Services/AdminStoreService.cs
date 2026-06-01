using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class AdminStoreService : IAdminStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminStoreService(IStoreRepository storeRepository, IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AdminStoreItemDto>> GetItemsAsync(
        StoreCategory? category,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _storeRepository.CountAllItemsAsync(category, includeInactive, cancellationToken);
        var items = await _storeRepository.GetAllItemsAsync(category, includeInactive, page, pageSize, cancellationToken);

        return new PagedResult<AdminStoreItemDto>
        {
            Items = items.Select(ToAdminDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<AdminStoreItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");
        return ToAdminDto(item);
    }

    public async Task<AdminStoreItemDto> CreateAsync(CreateStoreItemRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Name, request.AssetUrl, request.CoinPrice, request.RealMoneyPriceVnd);

        var item = new StoreItem
        {
            Category = request.Category,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AssetUrl = request.AssetUrl.Trim(),
            IsPremium = request.IsPremium,
            CoinPrice = NormalizeCoinPrice(request.CoinPrice),
            RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd),
            IsActive = request.IsActive
        };

        await _storeRepository.AddStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(item);
    }

    public async Task<AdminStoreItemDto> UpdateAsync(Guid id, UpdateStoreItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        ValidateRequest(request.Name, request.AssetUrl, request.CoinPrice, request.RealMoneyPriceVnd);

        item.Category = request.Category;
        item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim();
        item.AssetUrl = request.AssetUrl.Trim();
        item.IsPremium = request.IsPremium;
        item.CoinPrice = NormalizeCoinPrice(request.CoinPrice);
        item.RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd);
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        item.IsActive = false;
        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateRequest(string name, string assetUrl, int? coinPrice, long? realMoneyPriceVnd)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name is required.");

        if (string.IsNullOrWhiteSpace(assetUrl))
            throw new ValidationException("AssetUrl is required.");

        if (coinPrice is < 0)
            throw new ValidationException("CoinPrice cannot be negative.");

        if (realMoneyPriceVnd is < 0)
            throw new ValidationException("RealMoneyPriceVnd cannot be negative.");

    }

    private static int? NormalizeCoinPrice(int? coinPrice) =>
        coinPrice is null or <= 0 ? null : coinPrice;

    private static long? NormalizeMoneyPrice(long? price) =>
        price is null or <= 0 ? null : price;

    private static AdminStoreItemDto ToAdminDto(StoreItem x) =>
        new(x.Id, x.Category, x.Name, x.Description, x.AssetUrl, x.IsPremium, x.CoinPrice, x.RealMoneyPriceVnd, x.IsActive, x.CreatedAt, x.UpdatedAt);
}
