using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class PaymentTransactionRepository : IPaymentTransactionRepository
{
    private readonly AppDbContext _context;

    public PaymentTransactionRepository(AppDbContext context) => _context = context;

    public Task<PaymentTransaction?> GetByTransactionCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _context.PaymentTransactions.Include(x => x.User).FirstOrDefaultAsync(x => x.TransactionCode == code, cancellationToken);

    public async Task AddAsync(PaymentTransaction tx, CancellationToken cancellationToken = default) =>
        await _context.PaymentTransactions.AddAsync(tx, cancellationToken);

    public Task UpdateAsync(PaymentTransaction tx, CancellationToken cancellationToken = default)
    {
        _context.PaymentTransactions.Update(tx);
        return Task.CompletedTask;
    }
}

