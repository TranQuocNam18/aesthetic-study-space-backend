using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
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
}

