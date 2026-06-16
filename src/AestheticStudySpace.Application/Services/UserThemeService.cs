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
            PreviewUrl = request.PreviewUrl?.Trim(),
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

    public async Task<UserThemeSubmissionDto> UpdateThemeAsync(
        Guid userId,
        Guid id,
        SubmitThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name is required.");
        if (string.IsNullOrWhiteSpace(request.AssetUrl))
            throw new ValidationException("AssetUrl is required.");
        if (request.CoinPrice is < 0)
            throw new ValidationException("CoinPrice cannot be negative.");
        if (request.RealMoneyPriceVnd is < 0)
            throw new ValidationException("RealMoneyPriceVnd cannot be negative.");

        var item = await _storeRepository.GetUserSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Theme submission not found.");

        if (item.Status == StoreItemStatus.Approved)
            throw new ValidationException("Cannot update an approved theme that is live in the store.");

        item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim();
        item.AssetUrl = request.AssetUrl.Trim();
        item.PreviewUrl = request.PreviewUrl?.Trim();
        item.ThemeStickerItemId = request.ThemeStickerItemId;
        item.ThemeBackgroundItemId = request.ThemeBackgroundItemId;
        item.ThemeEffectItemId = request.ThemeEffectItemId;
        item.ThemeAmbientSoundItemId = request.ThemeAmbientSoundItemId;
        item.CoinPrice = request.CoinPrice is > 0 ? request.CoinPrice : null;
        item.RealMoneyPriceVnd = request.RealMoneyPriceVnd is > 0 ? request.RealMoneyPriceVnd : null;

        // Reset status to PendingReview and clear rejection notes when modified
        item.Status = StoreItemStatus.PendingReview;
        item.RejectionNote = null;
        item.UpdatedAt = DateTime.UtcNow;

        ValidateThemeComponents(item.ThemeStickerItemId, item.ThemeBackgroundItemId, item.ThemeEffectItemId, item.ThemeAmbientSoundItemId);

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task<UserThemeSubmissionDto> PatchThemeAsync(
        Guid userId,
        Guid id,
        PatchThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetUserSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Theme submission not found.");

        if (item.Status == StoreItemStatus.Approved)
            throw new ValidationException("Cannot update an approved theme that is live in the store.");

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Name cannot be empty.");
            item.Name = request.Name.Trim();
        }

        if (request.Description is not null)
            item.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        if (request.AssetUrl is not null)
        {
            if (string.IsNullOrWhiteSpace(request.AssetUrl))
                throw new ValidationException("AssetUrl cannot be empty.");
            item.AssetUrl = request.AssetUrl.Trim();
        }

        if (request.PreviewUrl is not null)
            item.PreviewUrl = string.IsNullOrWhiteSpace(request.PreviewUrl) ? null : request.PreviewUrl.Trim();

        if (request.ThemeStickerItemId != default)
            item.ThemeStickerItemId = request.ThemeStickerItemId;

        if (request.ThemeBackgroundItemId != default)
            item.ThemeBackgroundItemId = request.ThemeBackgroundItemId;

        if (request.ThemeEffectItemId != default)
            item.ThemeEffectItemId = request.ThemeEffectItemId;

        if (request.ThemeAmbientSoundItemId != default)
            item.ThemeAmbientSoundItemId = request.ThemeAmbientSoundItemId;

        if (request.CoinPrice is not null)
            item.CoinPrice = request.CoinPrice > 0 ? request.CoinPrice : null;

        if (request.RealMoneyPriceVnd is not null)
            item.RealMoneyPriceVnd = request.RealMoneyPriceVnd > 0 ? request.RealMoneyPriceVnd : null;

        // Reset status to PendingReview and clear rejection notes when modified
        item.Status = StoreItemStatus.PendingReview;
        item.RejectionNote = null;
        item.UpdatedAt = DateTime.UtcNow;

        ValidateThemeComponents(item.ThemeStickerItemId, item.ThemeBackgroundItemId, item.ThemeEffectItemId, item.ThemeAmbientSoundItemId);

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    private static UserThemeSubmissionDto ToDto(StoreItem x) =>
        new(x.Id, x.Name, x.Description, x.AssetUrl, x.PreviewUrl,
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
        var provided = new[] { stickerItemId, backgroundItemId, effectItemId, ambientSoundItemId }
            .Count(id => id is not null);

        if (provided < 2)
            throw new ValidationException("Theme submissions must include at least 2 different component types (sticker, background, effect, or ambient sound).");
    }
}
