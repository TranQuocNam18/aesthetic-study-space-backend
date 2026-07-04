using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Admin;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly IAdminAnalyticsRepository _analyticsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentTransactionRepository _paymentTxRepository;
    private readonly IPomodoroRepository _pomodoroRepository;
    private readonly ITodoRepository _todoRepository;
    private readonly IPaymentFulfillmentService _fulfillmentService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(
        IAdminRepository adminRepository,
        IAdminAnalyticsRepository analyticsRepository,
        IUserRepository userRepository,
        IPaymentTransactionRepository paymentTxRepository,
        IPomodoroRepository pomodoroRepository,
        ITodoRepository todoRepository,
        IPaymentFulfillmentService fulfillmentService,
        IUnitOfWork unitOfWork)
    {
        _adminRepository = adminRepository;
        _analyticsRepository = analyticsRepository;
        _userRepository = userRepository;
        _paymentTxRepository = paymentTxRepository;
        _pomodoroRepository = pomodoroRepository;
        _todoRepository = todoRepository;
        _fulfillmentService = fulfillmentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (users, total) = await _adminRepository.GetUsersAsync(page, pageSize, cancellationToken);
        return new PagedResult<AdminUserDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = users.Select(ToDto).ToList()
        };
    }

    public async Task<AdminUserDto> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _adminRepository.GetUserByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");
        return ToDto(user);
    }

    public async Task BanUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");
        user.IsBanned = true;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnbanUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");
        user.IsBanned = false;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Soft-delete: mark IsDeleted through DbContext delete interception.
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        // We don't have a dedicated delete on IUserRepository; delete via EF by attaching in UoW later.
        // Minimal approach: ban + scramble email to prevent re-login while keeping data.
        user.IsBanned = true;
        user.Email = $"deleted-{user.Id}@aestheticstudy.space";
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdminUserDto> UpdateUserTierAsync(Guid id, string tier, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<AccountTier>(tier, true, out var accountTier))
            throw new ValidationException($"Invalid tier '{tier}'. Valid values: Free, Premium.");

        var user = await _adminRepository.GetUserByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.AccountTier = accountTier;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public Task<AdminOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default) =>
        _analyticsRepository.GetOverviewAsync(cancellationToken);

    public Task<IReadOnlyList<AdminDateCountDto>> GetUserGrowthAsync(int days, CancellationToken cancellationToken = default) =>
        _analyticsRepository.GetUserGrowthAsync(days, cancellationToken);

    public Task<AdminFeatureUsageDto> GetFeatureUsageAsync(CancellationToken cancellationToken = default) =>
        _analyticsRepository.GetFeatureUsageAsync(cancellationToken);

    public Task<AdminRevenueSummaryDto> GetRevenueSummaryAsync(CancellationToken cancellationToken = default) =>
        _analyticsRepository.GetRevenueSummaryAsync(cancellationToken);

    public Task<IReadOnlyList<AdminRevenueTrendDto>> GetRevenueTrendAsync(int days, CancellationToken cancellationToken = default) =>
        _analyticsRepository.GetRevenueTrendAsync(days, cancellationToken);

    public async Task<PagedResult<AdminPaymentTransactionDto>> GetPaymentsAsync(
        string? search,
        PaymentProvider? provider,
        PaymentStatus? status,
        PaymentPurpose? purpose,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (payments, total) = await _adminRepository.GetPaymentsAsync(search, provider, status, purpose, fromDate, toDate, page, pageSize, cancellationToken);
        return new PagedResult<AdminPaymentTransactionDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = payments.Select(ToPaymentDto).ToList()
        };
    }

    public async Task<AdminPaymentTransactionDto> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tx = await _adminRepository.GetPaymentByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Payment transaction not found.");
        return ToPaymentDto(tx);
    }

    public async Task ManualFulfillPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tx = await _adminRepository.GetPaymentByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Payment transaction not found.");

        if (tx.Status != PaymentStatus.Succeeded)
        {
            tx.Status = PaymentStatus.Succeeded;
            tx.SucceededAt = DateTime.UtcNow;
            await _paymentTxRepository.UpdateAsync(tx, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await _fulfillmentService.FulfillIfNeededAsync(tx, cancellationToken);
    }

    private static AdminUserDto ToDto(Domain.Entities.User u) =>
        new(u.Id, u.Username, u.Email, u.Role.Name, u.AccountTier.ToString(), u.IsBanned, u.CoinsBalance, u.CreatedAt, u.LastLoginAt);

    private static AdminPaymentTransactionDto ToPaymentDto(PaymentTransaction tx) =>
        new(
            tx.Id,
            tx.UserId,
            tx.User?.Username ?? string.Empty,
            tx.User?.Email ?? string.Empty,
            tx.Provider,
            tx.Status,
            tx.Purpose,
            tx.TransactionCode,
            tx.Amount,
            tx.Currency,
            tx.ProviderPayloadJson,
            tx.MetadataJson,
            tx.IsFulfilled,
            tx.SucceededAt,
            tx.FailedAt,
            tx.CreatedAt);
}

