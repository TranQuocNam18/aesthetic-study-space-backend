using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class StoreService : IStoreService
{
    private readonly IUserRepository _userRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ICoinTransactionRepository _coinTransactionRepository;
    private readonly IMissionService _missionService;
    private readonly IUnitOfWork _unitOfWork;

    public StoreService(
        IUserRepository userRepository, 
        IStoreRepository storeRepository, 
        ICoinTransactionRepository coinTransactionRepository, 
        IMissionService missionService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _storeRepository = storeRepository;
        _coinTransactionRepository = coinTransactionRepository;
        _missionService = missionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<StoreItemDto>> GetCatalogAsync(
        StoreCategory? category,
        StoreThemeSource? themeSource,
        StoreCatalogScope scope,
        Guid? viewerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var ownedIds = viewerUserId is null
            ? null
            : await _storeRepository.GetOwnedStoreItemIdsAsync(viewerUserId.Value, cancellationToken);

        var total = await _storeRepository.CountActiveItemsAsync(category, themeSource, scope, cancellationToken);
        var items = await _storeRepository.GetActiveItemsAsync(category, themeSource, scope, page, pageSize, cancellationToken);

        var themeIds = items.Where(x => x.Category == StoreCategory.Theme).Select(x => x.Id).ToList();
        var allInline = new Dictionary<Guid, IReadOnlyList<StoreItem>>();
        foreach (var tid in themeIds)
        {
            var inline = await _storeRepository.GetInlineComponentsByThemeIdAsync(tid, cancellationToken);
            allInline[tid] = inline;
        }

        return new PagedResult<StoreItemDto>
        {
            Items = items.Select(x => ToDto(x, ownedIds, allInline.GetValueOrDefault(x.Id))).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<StoreItemDto> GetItemAsync(Guid itemId, Guid? viewerUserId, CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetActiveByIdAsync(itemId, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        HashSet<Guid>? ownedIds = null;
        if (viewerUserId is not null)
            ownedIds = await _storeRepository.GetOwnedStoreItemIdsAsync(viewerUserId.Value, cancellationToken);

        IReadOnlyList<StoreItem>? inline = null;
        if (item.Category == StoreCategory.Theme)
            inline = await _storeRepository.GetInlineComponentsByThemeIdAsync(item.Id, cancellationToken);

        return ToDto(item, ownedIds, inline);
    }

    public async Task<PagedResult<UserInventoryItemDto>> GetInventoryAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var total = await _storeRepository.CountInventoryAsync(userId, cancellationToken);
        var rows = await _storeRepository.GetInventoryAsync(userId, page, pageSize, cancellationToken);

        return new PagedResult<UserInventoryItemDto>
        {
            Items = rows.Select(ToInventoryDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<StorePurchaseResultDto> BuyWithCoinsAsync(Guid userId, BuyWithCoinsRequestDto request, CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(request.StoreItemId, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        if (!item.IsActive)
            throw new ValidationException("Store item is not active.");

        if (item.CoinPrice is null || item.CoinPrice <= 0)
            throw new ValidationException("This item cannot be purchased with coins.");

        if (await _storeRepository.HasInventoryAsync(userId, item.Id, cancellationToken))
            throw new ValidationException("Item already owned.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("User is banned.");

        if (item.IsPremium && user.AccountTier != AccountTier.Premium)
            throw new ForbiddenException("Premium subscription required.");

        if (user.CoinsBalance < item.CoinPrice.Value)
            throw new ValidationException("Not enough coins.");

        user.CoinsBalance -= item.CoinPrice.Value;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var purchase = new Purchase
        {
            UserId = userId,
            StoreItemId = item.Id,
            CoinsSpent = item.CoinPrice.Value,
            AmountVnd = null
        };
        await _storeRepository.AddPurchaseAsync(purchase, cancellationToken);

        await _storeRepository.AddInventoryAsync(new UserInventory
        {
            UserId = userId,
            StoreItemId = item.Id
        }, cancellationToken);

        // Trigger mission: Buy store item
        await _missionService.IncrementByTriggerKeyAsync(userId, "buy_store_item", 1, cancellationToken);

        await _coinTransactionRepository.AddAsync(new CoinTransaction
        {
            UserId = userId,
            Type = CoinTransactionType.Spent,
            Amount = item.CoinPrice.Value,
            Reason = $"Purchase:{item.Name}",
            RelatedPurchase = purchase
        }, cancellationToken);

        if (item.CreatorId.HasValue && item.CreatorId.Value != userId)
        {
            var creator = await _userRepository.GetByIdAsync(item.CreatorId.Value, cancellationToken);
            if (creator != null)
            {
                creator.CoinsBalance += item.CoinPrice.Value;
                await _userRepository.UpdateAsync(creator, cancellationToken);

                await _coinTransactionRepository.AddAsync(new CoinTransaction
                {
                    UserId = creator.Id,
                    Type = CoinTransactionType.Earned,
                    Amount = item.CoinPrice.Value,
                    Reason = $"Sold:{item.Name}",
                    RelatedPurchase = purchase
                }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StorePurchaseResultDto(true, item.Id, user.CoinsBalance);
    }

    public async Task<PagedResult<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(
        Guid userId,
        PurchaseHistoryKind? kind,
        PaymentStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var index = await _storeRepository.GetPurchaseHistoryIndexAsync(userId, cancellationToken);

        var purchaseIds = index.Where(x => x.IsPurchase).Select(x => x.Id).ToList();
        var paymentIds = index.Where(x => !x.IsPurchase).Select(x => x.Id).ToList();

        var purchases = await _storeRepository.GetPurchaseHistoryPurchasesAsync(userId, purchaseIds, cancellationToken);
        var subscriptionPayments = await _storeRepository.GetPurchaseHistorySubscriptionPaymentsAsync(userId, paymentIds, cancellationToken);

        var purchaseById = purchases.ToDictionary(x => x.Id);
        var paymentById = subscriptionPayments.ToDictionary(x => x.Id);

        // Build full history list in sorted order
        var allItems = index
            .Select(entry =>
            {
                if (entry.IsPurchase && purchaseById.TryGetValue(entry.Id, out var purchase))
                    return ToHistoryDto(purchase);
                if (!entry.IsPurchase && paymentById.TryGetValue(entry.Id, out var payment))
                    return ToHistoryDto(payment);
                return null;
            })
            .Where(x => x is not null)
            .Cast<PurchaseHistoryItemDto>()
            .ToList();

        // Apply filters in-memory
        if (kind is not null)
            allItems = allItems.Where(x => x.Kind == kind.Value).ToList();

        if (status is not null)
            allItems = allItems.Where(x => x.Status == status.Value).ToList();

        if (fromDate is not null)
            allItems = allItems.Where(x => x.PurchasedAt >= fromDate.Value).ToList();

        if (toDate is not null)
            allItems = allItems.Where(x => x.PurchasedAt <= toDate.Value).ToList();

        var total = allItems.Count;
        var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<PurchaseHistoryItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }


    private static UserInventoryItemDto ToInventoryDto(UserInventory row)
    {
        var item = row.StoreItem;
        return new UserInventoryItemDto(
            row.Id,
            item.Id,
            item.Category,
            item.ThemeSource,
            item.Name,
            item.Description,
            item.AssetUrl,
            item.PreviewUrl,
            item.ThemeStickerItemId,
            item.ThemeBackgroundItemId,
            item.ThemeEffectItemId,
            item.ThemeAmbientSoundItemId,
            item.IsPremium,
            row.AcquiredAt);
    }

    private static PurchaseHistoryItemDto ToHistoryDto(Purchase purchase)
    {
        var payment = purchase.PaymentTransaction;
        var item = purchase.StoreItem;

        if (item is not null)
        {
            return new PurchaseHistoryItemDto(
                purchase.Id,
                PurchaseHistoryKind.StoreItem,
                item.Name,
                item.Description,
                purchase.CoinsSpent,
                purchase.AmountVnd,
                purchase.Currency,
                payment?.Provider,
                payment?.TransactionCode,
                payment?.Status ?? PaymentStatus.Succeeded,
                item.Id,
                item.Category,
                item.ThemeSource,
                item.AssetUrl,
                item.ThemeStickerItemId,
                item.ThemeBackgroundItemId,
                item.ThemeEffectItemId,
                item.ThemeAmbientSoundItemId,
                purchase.CreatedAt);
        }

        if (payment?.Purpose == PaymentPurpose.Subscription)
        {
            return new PurchaseHistoryItemDto(
                purchase.Id,
                PurchaseHistoryKind.Subscription,
                "Premium Subscription",
                "30-day Premium access",
                null,
                purchase.AmountVnd ?? payment.Amount,
                purchase.Currency,
                payment.Provider,
                payment.TransactionCode,
                payment.Status,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                purchase.CreatedAt);
        }

        return new PurchaseHistoryItemDto(
            purchase.Id,
            PurchaseHistoryKind.CoinPack,
            "Coin Pack",
            payment is null ? "Purchased with coins" : "Coin pack purchase",
            null,
            purchase.AmountVnd,
            purchase.Currency,
            payment?.Provider,
            payment?.TransactionCode,
            payment?.Status ?? PaymentStatus.Succeeded,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            purchase.CreatedAt);
    }

    private static PurchaseHistoryItemDto ToHistoryDto(PaymentTransaction payment)
    {
        var kind = payment.Purpose switch
        {
            PaymentPurpose.Subscription => PurchaseHistoryKind.Subscription,
            PaymentPurpose.BuyCoins => PurchaseHistoryKind.CoinPack,
            PaymentPurpose.BuyAsset => PurchaseHistoryKind.StoreItem,
            _ => PurchaseHistoryKind.Subscription
        };

        var title = payment.Purpose switch
        {
            PaymentPurpose.Subscription => "Premium Subscription",
            PaymentPurpose.BuyCoins => "Coin Pack",
            PaymentPurpose.BuyAsset => "Store Item",
            _ => "Payment Transaction"
        };

        var description = payment.Purpose switch
        {
            PaymentPurpose.Subscription => "30-day Premium access",
            PaymentPurpose.BuyCoins => "Coin pack purchase",
            PaymentPurpose.BuyAsset => "Direct asset purchase",
            _ => null
        };

        return new PurchaseHistoryItemDto(
            payment.Id,
            kind,
            title,
            description,
            null,
            payment.Amount,
            payment.Currency,
            payment.Provider,
            payment.TransactionCode,
            payment.Status,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            payment.SucceededAt ?? payment.CreatedAt);
    }

    private static StoreItemDto ToDto(StoreItem x, HashSet<Guid>? ownedIds, IReadOnlyList<StoreItem>? inlineComponents = null) =>
        new(
            x.Id,
            x.Category,
            x.ThemeSource,
            x.Name,
            x.Description,
            x.AssetUrl,
            x.PreviewUrl,
            x.ThemeStickerItemId,
            x.ThemeBackgroundItemId,
            x.ThemeEffectItemId,
            x.ThemeAmbientSoundItemId,
            x.IsPremium,
            x.CoinPrice,
            x.RealMoneyPriceVnd,
            x.IsActive,
            ownedIds is null ? null : ownedIds.Contains(x.Id),
            x.CoinPrice is > 0,
            x.RealMoneyPriceVnd is > 0,
            inlineComponents?.Select(c => ToDto(c, ownedIds, null)).ToList());
}
