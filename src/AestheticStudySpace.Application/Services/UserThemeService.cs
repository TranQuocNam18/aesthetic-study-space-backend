using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class UserThemeService : IUserThemeService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserThemeService(
        IStoreRepository storeRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserThemeSubmissionDto> SubmitThemeAsync(
        Guid userId,
        SubmitThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name is required.");
        if (string.IsNullOrWhiteSpace(request.AssetUrl))
            throw new ValidationException("AssetUrl is required.");
        if (request.CoinPrice is < 0)
            throw new ValidationException("CoinPrice cannot be negative.");
        if (request.RealMoneyPriceVnd is < 0)
            throw new ValidationException("RealMoneyPriceVnd cannot be negative.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("Banned users cannot submit themes.");

        var item = new StoreItem
        {
            Category = StoreCategory.Theme,
            ThemeSource = StoreThemeSource.Community,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AssetUrl = request.AssetUrl.Trim(),
            ThemeStickerItemId = request.ThemeStickerItemId,
            ThemeBackgroundItemId = request.ThemeBackgroundItemId,
            ThemeEffectItemId = request.ThemeEffectItemId,
            ThemeAmbientSoundItemId = request.ThemeAmbientSoundItemId,
            IsPremium = false,
            CoinPrice = request.CoinPrice is > 0 ? request.CoinPrice : null,
            RealMoneyPriceVnd = request.RealMoneyPriceVnd is > 0 ? request.RealMoneyPriceVnd : null,
            IsActive = false,                      // hidden until approved
            CreatorId = userId,
            Status = StoreItemStatus.PendingReview
        };

        ValidateThemeComponents(item.ThemeStickerItemId, item.ThemeBackgroundItemId, item.ThemeEffectItemId, item.ThemeAmbientSoundItemId);

        await _storeRepository.AddStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task<PagedResult<UserThemeSubmissionDto>> GetMySubmissionsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var total = await _storeRepository.CountUserSubmissionsAsync(userId, cancellationToken);
        var items = await _storeRepository.GetUserSubmissionsAsync(userId, page, pageSize, cancellationToken);

        return new PagedResult<UserThemeSubmissionDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<UserThemeSubmissionDto> GetMySubmissionByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetUserSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Theme submission not found.");
        return ToDto(item);
    }

    public async Task WithdrawSubmissionAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetUserSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Theme submission not found.");

        if (item.Status == StoreItemStatus.Approved)
            throw new ValidationException("Cannot withdraw an approved theme that is live in the store.");

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static UserThemeSubmissionDto ToDto(StoreItem x) =>
        new(x.Id, x.Name, x.Description, x.AssetUrl,
            x.ThemeStickerItemId, x.ThemeBackgroundItemId, x.ThemeEffectItemId, x.ThemeAmbientSoundItemId,
            x.CoinPrice, x.RealMoneyPriceVnd,
            x.ThemeSource ?? StoreThemeSource.Community,
            x.Status, x.RejectionNote,
            x.CreatedAt, x.ReviewedAt);

    private static void ValidateThemeComponents(
        Guid? stickerItemId,
        Guid? backgroundItemId,
        Guid? effectItemId,
        Guid? ambientSoundItemId)
    {
        if (stickerItemId is null || backgroundItemId is null || effectItemId is null || ambientSoundItemId is null)
            throw new ValidationException("Theme submissions must include sticker, background, effect, and ambient sound components.");
    }
}
