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
    private readonly INotificationService _notificationService;
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
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserComponentSubmissionDto> SubmitComponentAsync(
        Guid userId,
        SubmitComponentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!request.IsAgreedToTerms)
            throw new ValidationException("You must agree to the transaction terms of service before submitting components.");

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
            Status = StoreItemStatus.PendingTransaction,
            ParentThemeId = null,    // standalone submission
            BankAccountNumber = request.BankAccountNumber?.Trim(),
            BankName = request.BankName?.Trim(),
            BankAccountOwnerName = request.BankAccountOwnerName?.Trim(),
            RequestedCoinPrice = request.RequestedCoinPrice,
            RequestedRealMoneyPriceVnd = request.RequestedRealMoneyPriceVnd
        };

        await _storeRepository.AddStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create Web Notification for admin
        await _notificationService.CreateNotificationAsync(
            userId: null,
            isForAdmin: true,
            title: "New Component Transaction Request",
            message: $"User {user.Username} submitted component '{item.Name}' ({item.Category}) for buyout transaction.",
            cancellationToken);

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

        if (item.Status == StoreItemStatus.PurchasedPendingPricing)
            throw new ValidationException("Cannot withdraw a component that has already been bought out by the admin.");

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

        if (item.Status == StoreItemStatus.PendingTransaction || item.Status == StoreItemStatus.PurchasedPendingPricing)
            throw new ValidationException("Cannot update a component request that is currently pending transaction review or buyout processing.");

        item.Category = request.Category;
        item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim();
        item.AssetUrl = request.AssetUrl.Trim();
        item.PreviewUrl = request.PreviewUrl?.Trim();
        item.CoinPrice = request.CoinPrice is > 0 ? request.CoinPrice : null;
        item.RealMoneyPriceVnd = request.RealMoneyPriceVnd is > 0 ? request.RealMoneyPriceVnd : null;
        item.BankAccountNumber = request.BankAccountNumber?.Trim();
        item.BankName = request.BankName?.Trim();
        item.BankAccountOwnerName = request.BankAccountOwnerName?.Trim();
        item.RequestedCoinPrice = request.RequestedCoinPrice;
        item.RequestedRealMoneyPriceVnd = request.RequestedRealMoneyPriceVnd;

        // Reset to PendingTransaction on any update
        item.Status = StoreItemStatus.PendingTransaction;
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

        if (item.Status == StoreItemStatus.PendingTransaction || item.Status == StoreItemStatus.PurchasedPendingPricing)
            throw new ValidationException("Cannot update a component request that is currently pending transaction review or buyout processing.");

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

        if (request.BankAccountNumber is not null)
            item.BankAccountNumber = string.IsNullOrWhiteSpace(request.BankAccountNumber) ? null : request.BankAccountNumber.Trim();

        if (request.BankName is not null)
            item.BankName = string.IsNullOrWhiteSpace(request.BankName) ? null : request.BankName.Trim();

        if (request.BankAccountOwnerName is not null)
            item.BankAccountOwnerName = string.IsNullOrWhiteSpace(request.BankAccountOwnerName) ? null : request.BankAccountOwnerName.Trim();

        if (request.RequestedCoinPrice is not null)
            item.RequestedCoinPrice = request.RequestedCoinPrice > 0 ? request.RequestedCoinPrice : null;

        if (request.RequestedRealMoneyPriceVnd is not null)
            item.RequestedRealMoneyPriceVnd = request.RequestedRealMoneyPriceVnd > 0 ? request.RequestedRealMoneyPriceVnd : null;

        // Reset to PendingTransaction on any update
        item.Status = StoreItemStatus.PendingTransaction;
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
            x.CreatedAt, x.ReviewedAt,
            x.BankAccountNumber, x.BankName, x.BankAccountOwnerName,
            x.RequestedCoinPrice, x.RequestedRealMoneyPriceVnd,
            x.IsBoughtByAdmin);
}
