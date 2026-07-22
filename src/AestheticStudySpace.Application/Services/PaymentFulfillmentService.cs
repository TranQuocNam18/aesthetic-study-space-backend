using System.Text.Json;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class PaymentFulfillmentService : IPaymentFulfillmentService
{
    private readonly IUserRepository _userRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ICoinTransactionRepository _coinTransactionRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentFulfillmentService(
        IUserRepository userRepository,
        IStoreRepository storeRepository,
        ICoinTransactionRepository coinTransactionRepository,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _storeRepository = storeRepository;
        _coinTransactionRepository = coinTransactionRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task FulfillIfNeededAsync(PaymentTransaction tx, CancellationToken cancellationToken = default)
    {
        if (tx.Status != PaymentStatus.Succeeded)
            return;

        if (tx.IsFulfilled)
            return;

        var user = await _userRepository.GetByIdAsync(tx.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("User is banned.");

        switch (tx.Purpose)
        {
            case PaymentPurpose.Subscription:
                await FulfillSubscriptionAsync(user, tx, cancellationToken);
                break;

            case PaymentPurpose.BuyCoins:
                await FulfillBuyCoinsAsync(user, tx, cancellationToken);
                break;

            case PaymentPurpose.BuyAsset:
                await FulfillBuyAssetAsync(user, tx, cancellationToken);
                break;

            default:
                throw new ValidationException("Unsupported payment purpose.");
        }

        tx.IsFulfilled = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task FulfillSubscriptionAsync(User user, PaymentTransaction tx, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var durationDays = 30;

        var active = await _subscriptionRepository.GetActiveByUserAsync(user.Id, cancellationToken);
        if (active is null)
        {
            active = new Subscription
            {
                UserId = user.Id,
                StartsAt = now,
                EndsAt = now.AddDays(durationDays),
                IsActive = true,
                PaymentTransactionId = tx.Id
            };
            await _subscriptionRepository.AddAsync(active, cancellationToken);
        }
        else
        {
            active.EndsAt = active.EndsAt.AddDays(durationDays);
            active.PaymentTransactionId = tx.Id;
            await _subscriptionRepository.UpdateAsync(active, cancellationToken);
        }

        user.AccountTier = AccountTier.Premium;
        await _userRepository.UpdateAsync(user, cancellationToken);

        if (!await _storeRepository.HasPurchaseForPaymentAsync(tx.Id, cancellationToken))
        {
            await _storeRepository.AddPurchaseAsync(new Purchase
            {
                UserId = user.Id,
                AmountVnd = tx.Amount,
                PaymentTransactionId = tx.Id
            }, cancellationToken);
        }
    }

    private async Task FulfillBuyCoinsAsync(User user, PaymentTransaction tx, CancellationToken cancellationToken)
    {
        var meta = ParseMetadata(tx.MetadataJson);
        if (!meta.TryGetValue("coinsAmount", out var coinsStr) || !int.TryParse(coinsStr, out var coins) || coins <= 0)
            throw new ValidationException("Missing coinsAmount metadata.");

        user.CoinsBalance += coins;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var purchase = new Purchase
        {
            UserId = user.Id,
            AmountVnd = tx.Amount,
            CoinsSpent = null,
            PaymentTransactionId = tx.Id
        };
        await _storeRepository.AddPurchaseAsync(purchase, cancellationToken);

        await _coinTransactionRepository.AddAsync(new CoinTransaction
        {
            UserId = user.Id,
            Type = CoinTransactionType.Adjusted,
            Amount = coins,
            Reason = "BuyCoins",
            RelatedPurchase = purchase
        }, cancellationToken);
    }

    private async Task FulfillBuyAssetAsync(User user, PaymentTransaction tx, CancellationToken cancellationToken)
    {
        var meta = ParseMetadata(tx.MetadataJson);
        if (!meta.TryGetValue("storeItemId", out var storeItemIdStr) || !Guid.TryParse(storeItemIdStr, out var storeItemId))
            throw new ValidationException("Missing storeItemId metadata.");

        var item = await _storeRepository.GetByIdAsync(storeItemId, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        if (await _storeRepository.HasInventoryAsync(user.Id, storeItemId, cancellationToken))
            return; // idempotent: already owned

        // Allow any user (free and premium) to purchase items
        // if (item.IsPremium && user.AccountTier != AccountTier.Premium)
        //     throw new ForbiddenException("Premium subscription required.");

        var purchase = new Purchase
        {
            UserId = user.Id,
            StoreItemId = storeItemId,
            AmountVnd = tx.Amount,
            PaymentTransactionId = tx.Id
        };
        await _storeRepository.AddPurchaseAsync(purchase, cancellationToken);

        await _storeRepository.AddInventoryAsync(new UserInventory
        {
            UserId = user.Id,
            StoreItemId = storeItemId
        }, cancellationToken);
    }

    private static Dictionary<string, string> ParseMetadata(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return dict ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}

