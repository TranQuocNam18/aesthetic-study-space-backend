using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;

    public SubscriptionRepository(AppDbContext context) => _context = context;

    public Task<Subscription?> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _context.Subscriptions.FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive && x.EndsAt > DateTime.UtcNow, cancellationToken);

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default) =>
        await _context.Subscriptions.AddAsync(subscription, cancellationToken);

    public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Update(subscription);
        return Task.CompletedTask;
    }
}

