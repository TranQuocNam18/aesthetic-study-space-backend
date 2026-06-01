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
    private readonly IUnitOfWork _unitOfWork;

    public StoreService(IUserRepository userRepository, IStoreRepository storeRepository, ICoinTransactionRepository coinTransactionRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _storeRepository = storeRepository;
        _coinTransactionRepository = coinTransactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<StoreItemDto>> GetItemsAsync(StoreCategory? category, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = await _storeRepository.GetActiveItemsAsync(category, page, pageSize, cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<object> BuyWithCoinsAsync(Guid userId, BuyWithCoinsRequestDto request, CancellationToken cancellationToken = default)
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

        var inventory = new UserInventory
        {
            UserId = userId,
            StoreItemId = item.Id
        };
        await _storeRepository.AddInventoryAsync(inventory, cancellationToken);

        var coinTx = new CoinTransaction
        {
            UserId = userId,
            Type = CoinTransactionType.Spent,
            Amount = item.CoinPrice.Value,
            Reason = $"Purchase:{item.Name}",
            RelatedPurchase = purchase
        };

        await _coinTransactionRepository.AddAsync(coinTx, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new
        {
            purchased = true,
            storeItemId = item.Id,
            remainingCoins = user.CoinsBalance
        };
    }

    private static StoreItemDto ToDto(StoreItem x) =>
        new(x.Id, x.Category, x.Name, x.Description, x.AssetUrl, x.IsPremium, x.CoinPrice, x.RealMoneyPriceVnd, x.IsActive);
}

