using AestheticStudySpace.Application.DTOs.Payments;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUserRepository _userRepository;
    private readonly IPaymentTransactionRepository _paymentTxRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionService(
        IUserRepository userRepository,
        IPaymentTransactionRepository paymentTxRepository,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _paymentTxRepository = paymentTxRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> UpgradeAsync(Guid userId, SubscriptionUpgradeRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionCode))
            throw new ValidationException("TransactionCode is required.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("User is banned.");

        var tx = await _paymentTxRepository.GetByTransactionCodeAsync(request.TransactionCode.Trim(), cancellationToken)
            ?? throw new NotFoundException("Transaction not found.");

        if (tx.UserId != userId)
            throw new ForbiddenException("You do not own this transaction.");

        if (tx.Status != PaymentStatus.Succeeded)
            throw new ValidationException("Payment is not completed.");

        // Must be a real-money payment (coins purchases don't create PaymentTransaction)
        if (tx.Amount <= 0)
            throw new ValidationException("Invalid transaction amount.");

        var active = await _subscriptionRepository.GetActiveByUserAsync(userId, cancellationToken);
        var now = DateTime.UtcNow;
        var durationDays = 30;

        if (active is null)
        {
            active = new Subscription
            {
                UserId = userId,
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new
        {
            upgraded = true,
            tier = user.AccountTier.ToString(),
            subscriptionEndsAt = active.EndsAt
        };
    }

    public async Task<object> ActivateTrialAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("User is banned.");

        if (user.AccountTier == AccountTier.Premium)
            throw new ValidationException("User is already a Premium member.");

        var alreadyUsedTrial = await _subscriptionRepository.HasUsedTrialAsync(userId, cancellationToken);
        if (alreadyUsedTrial)
            throw new ValidationException("You have already used your free trial.");

        var now = DateTime.UtcNow;
        var trialSubscription = new Subscription
        {
            UserId = userId,
            StartsAt = now,
            EndsAt = now.AddDays(5),
            IsActive = true,
            PaymentTransactionId = null
        };

        await _subscriptionRepository.AddAsync(trialSubscription, cancellationToken);

        user.AccountTier = AccountTier.Premium;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new
        {
            trialActivated = true,
            tier = user.AccountTier.ToString(),
            subscriptionEndsAt = trialSubscription.EndsAt
        };
    }
}

