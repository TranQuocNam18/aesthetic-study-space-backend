using AestheticStudySpace.Application.DTOs.LuckyDraw;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class LuckyDrawService : ILuckyDrawService
{
    private readonly IUserLuckyDrawRepository _luckyDrawRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICoinTransactionRepository _coinTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    // Weighted rewards: (Coins, Description, Weight)
    private static readonly (int Coins, string Description, double Weight)[] RewardPool = new[]
    {
        (10, "Lucky Prize: 10 Coins", 50.0),
        (20, "Bronze Prize: 20 Coins", 30.0),
        (50, "Silver Prize: 50 Coins", 10.0),
        (100, "Gold Prize: 100 Coins", 5.0),
        (200, "Jackpot Prize: 200 Coins!", 1.0),
        (500, "Big Jackpot Prize: 500 Coins!", 0.2)
    };

    public LuckyDrawService(
        IUserLuckyDrawRepository luckyDrawRepository,
        IUserRepository userRepository,
        ICoinTransactionRepository coinTransactionRepository,
        IUnitOfWork unitOfWork)
    {
        _luckyDrawRepository = luckyDrawRepository;
        _userRepository = userRepository;
        _coinTransactionRepository = coinTransactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LuckyDrawStatusDto> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var drawsToday = await _luckyDrawRepository.GetDrawsForDateAsync(userId, today, cancellationToken);

        var isPremium = user.AccountTier == AccountTier.Premium;
        var maxDrawsToday = isPremium ? 2 : 1;
        var remainingDrawsToday = Math.Max(0, maxDrawsToday - drawsToday.Count);

        var historyDtos = drawsToday
            .Select(x => new LuckyDrawHistoryItemDto(x.Id, x.RewardCoins, x.RewardDescription, x.CreatedAt))
            .ToList();

        return new LuckyDrawStatusDto(
            remainingDrawsToday,
            maxDrawsToday,
            remainingDrawsToday > 0,
            isPremium,
            historyDtos);
    }

    public async Task<LuckyDrawResultDto> SpinAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("User is banned.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var drawsTodayCount = await _luckyDrawRepository.CountDrawsForDateAsync(userId, today, cancellationToken);

        var isPremium = user.AccountTier == AccountTier.Premium;
        var maxDrawsToday = isPremium ? 2 : 1;

        if (drawsTodayCount >= maxDrawsToday)
        {
            throw new ValidationException(isPremium 
                ? "You have used all 2 daily free draws for today." 
                : "You have used your daily free draw. Upgrade to Premium for 2 draws per day!");
        }

        var reward = PickReward();

        user.CoinsBalance += reward.Coins;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var luckyDrawRecord = new UserLuckyDraw
        {
            UserId = userId,
            DrawDate = today,
            RewardCoins = reward.Coins,
            RewardDescription = reward.Description
        };
        await _luckyDrawRepository.AddAsync(luckyDrawRecord, cancellationToken);

        await _coinTransactionRepository.AddAsync(new CoinTransaction
        {
            UserId = userId,
            Type = CoinTransactionType.Earned,
            Amount = reward.Coins,
            Reason = $"LuckyDraw:{reward.Coins}Coins"
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var remainingDrawsToday = Math.Max(0, maxDrawsToday - (drawsTodayCount + 1));

        return new LuckyDrawResultDto(
            reward.Coins,
            reward.Description,
            remainingDrawsToday,
            user.CoinsBalance);
    }

    private static (int Coins, string Description) PickReward()
    {
        var totalWeight = RewardPool.Sum(x => x.Weight);
        var roll = Random.Shared.NextDouble() * totalWeight;
        var currentSum = 0.0;

        foreach (var item in RewardPool)
        {
            currentSum += item.Weight;
            if (roll < currentSum)
            {
                return (item.Coins, item.Description);
            }
        }

        return (RewardPool[0].Coins, RewardPool[0].Description);
    }
}
