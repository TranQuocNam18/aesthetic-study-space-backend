using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class UserLuckyDrawRepository : IUserLuckyDrawRepository
{
    private readonly AppDbContext _context;

    public UserLuckyDrawRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserLuckyDraw>> GetDrawsForDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _context.UserLuckyDraws
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.DrawDate == date)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountDrawsForDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _context.UserLuckyDraws
            .CountAsync(x => x.UserId == userId && x.DrawDate == date, cancellationToken);
    }

    public async Task AddAsync(UserLuckyDraw luckyDraw, CancellationToken cancellationToken = default)
    {
        await _context.UserLuckyDraws.AddAsync(luckyDraw, cancellationToken);
    }
}
