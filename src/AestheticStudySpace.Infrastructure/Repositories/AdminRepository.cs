using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _context;

    public AdminRepository(AppDbContext context) => _context = context;

    public async Task<(IReadOnlyList<User> users, int total)> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _context.Users.AsNoTracking().Include(u => u.Role).OrderByDescending(u => u.CreatedAt);
        var total = await query.CountAsync(cancellationToken);
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (users, total);
    }

    public Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users.AsNoTracking().Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<PaymentTransaction> payments, int total)> GetPaymentsAsync(
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
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _context.PaymentTransactions
            .Include(t => t.User)
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim();
            query = query.Where(t => 
                t.TransactionCode.Contains(cleanSearch) || 
                t.User.Username.Contains(cleanSearch) || 
                t.User.Email.Contains(cleanSearch));
        }

        if (provider is not null)
            query = query.Where(t => t.Provider == provider.Value);

        if (status is not null)
            query = query.Where(t => t.Status == status.Value);

        if (purpose is not null)
            query = query.Where(t => t.Purpose == purpose.Value);

        if (fromDate is not null)
            query = query.Where(t => t.CreatedAt >= fromDate.Value);

        if (toDate is not null)
            query = query.Where(t => t.CreatedAt <= toDate.Value);

        var total = await query.CountAsync(cancellationToken);
        var payments = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (payments, total);
    }

    public Task<PaymentTransaction?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.PaymentTransactions
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
}

