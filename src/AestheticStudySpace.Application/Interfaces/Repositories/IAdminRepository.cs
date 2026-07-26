using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IAdminRepository
{
    Task<(IReadOnlyList<User> users, int total)> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<PaymentTransaction> payments, int total)> GetPaymentsAsync(
        string? search,
        PaymentProvider? provider,
        PaymentStatus? status,
        PaymentPurpose? purpose,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PaymentTransaction?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeletePaymentTransactionsAsync(List<Guid> transactionIds, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetPaymentIdsByProviderAsync(PaymentProvider provider, CancellationToken cancellationToken = default);
}

