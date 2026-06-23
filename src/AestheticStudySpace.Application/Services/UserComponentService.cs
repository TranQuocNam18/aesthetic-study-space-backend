using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class UserComponentService : IUserComponentService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly IReadOnlySet<StoreCategory> AllowedCategories = new HashSet<StoreCategory>
    {
        StoreCategory.Sticker,
        StoreCategory.Background,
        StoreCategory.Effect,
        StoreCategory.AmbientSound
    };

    public UserComponentService(
        IStoreRepository storeRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserComponentSubmissionDto> SubmitComponentAsync(
        Guid userId,
        SubmitComponentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Category, request.Name, request.AssetUrl, request.CoinPrice, request.RealMoneyPriceVnd);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("Banned users cannot submit components.");

        var item = new StoreItem
        {
            Category = request.Category,
            ThemeSource = null, // components are never Theme
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AssetUrl = request.AssetUrl.Trim(),
            PreviewUrl = request.PreviewUrl?.Trim(),
            IsPremium = false,
            CoinPrice = request.CoinPrice is > 0 ? request.CoinPrice : null,
            RealMoneyPriceVnd = request.RealMoneyPriceVnd is > 0 ? request.RealMoneyPriceVnd : null,
            IsActive = false,       // hidden until approved
            CreatorId = userId,
            Status = StoreItemStatus.PendingReview,
            ParentThemeId = null    // standalone submission
        };

        await _storeRepository.AddStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task<PagedResult<UserComponentSubmissionDto>> GetMySubmissionsAsync(
        Guid userId,
        StoreCategory? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var total = await _storeRepository.CountUserComponentSubmissionsAsync(userId, category, cancellationToken);
        var items = await _storeRepository.GetUserComponentSubmissionsAsync(userId, category, page, pageSize, cancellationToken);

        return new PagedResult<UserComponentSubmissionDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<UserComponentSubmissionDto> GetMySubmissionByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetUserComponentSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Component submission not found.");
        return ToDto(item);
    }

    public async Task WithdrawSubmissionAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetUserComponentSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Component submission not found.");

        if (item.Status == StoreItemStatus.Approved)
            throw new ValidationException("Cannot withdraw an approved component that is live in the store.");

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserComponentSubmissionDto> UpdateAsync(
        Guid userId,
        Guid id,
        SubmitComponentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Category, request.Name, request.AssetUrl, request.CoinPrice, request.RealMoneyPriceVnd);

        var item = await _storeRepository.GetUserComponentSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Component submission not found.");

        if (item.Status == StoreItemStatus.Approved)
            throw new ValidationException("Cannot update an approved component that is live in the store.");

        item.Category = request.Category;
        item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim();
        item.AssetUrl = request.AssetUrl.Trim();
        item.PreviewUrl = request.PreviewUrl?.Trim();
        item.CoinPrice = request.CoinPrice is > 0 ? request.CoinPrice : null;
        item.RealMoneyPriceVnd = request.RealMoneyPriceVnd is > 0 ? request.RealMoneyPriceVnd : null;

        // Reset to PendingReview on any update
        item.Status = StoreItemStatus.PendingReview;
        item.RejectionNote = null;
        item.UpdatedAt = DateTime.UtcNow;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task<UserComponentSubmissionDto> PatchAsync(
        Guid userId,
        Guid id,
        PatchComponentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetUserComponentSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Component submission not found.");

        if (item.Status == StoreItemStatus.Approved)
            throw new ValidationException("Cannot update an approved component that is live in the store.");

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

        if (request.CoinPrice is not null)
            item.CoinPrice = request.CoinPrice > 0 ? request.CoinPrice : null;

        if (request.RealMoneyPriceVnd is not null)
            item.RealMoneyPriceVnd = request.RealMoneyPriceVnd > 0 ? request.RealMoneyPriceVnd : null;

        // Reset to PendingReview on any update
        item.Status = StoreItemStatus.PendingReview;
        item.RejectionNote = null;
        item.UpdatedAt = DateTime.UtcNow;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static void ValidateRequest(StoreCategory category, string name, string assetUrl, int? coinPrice, long? realMoneyPriceVnd)
    {
        if (!AllowedCategories.Contains(category))
            throw new ValidationException(
                $"Category '{category}' is not allowed for standalone component submissions. " +
                "Use the theme submission endpoints to submit a Theme.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name is required.");

        if (string.IsNullOrWhiteSpace(assetUrl))
            throw new ValidationException("AssetUrl is required.");

        if (coinPrice is < 0)
            throw new ValidationException("CoinPrice cannot be negative.");

        if (realMoneyPriceVnd is < 0)
            throw new ValidationException("RealMoneyPriceVnd cannot be negative.");
    }

    internal static UserComponentSubmissionDto ToDto(StoreItem x) =>
        new(x.Id, x.Category, x.Name, x.Description, x.AssetUrl, x.PreviewUrl,
            x.CoinPrice, x.RealMoneyPriceVnd,
            x.Status, x.RejectionNote,
            x.CreatedAt, x.ReviewedAt);
}
