using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IPaymentTransactionRepository
{
    Task<PaymentTransaction?> GetByTransactionCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(PaymentTransaction tx, CancellationToken cancellationToken = default);
    Task UpdateAsync(PaymentTransaction tx, CancellationToken cancellationToken = default);
}

