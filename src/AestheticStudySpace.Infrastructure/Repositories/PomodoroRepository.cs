using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class PomodoroRepository : IPomodoroRepository
{
    private readonly AppDbContext _context;

    public PomodoroRepository(AppDbContext context) => _context = context;

    public Task<PomodoroSession?> GetActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _context.PomodoroSessions.FirstOrDefaultAsync(p => p.UserId == userId && p.EndTime == null, cancellationToken);

    public Task<PomodoroSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.PomodoroSessions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PomodoroSession>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.PomodoroSessions
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.EndTime != null)
            .OrderByDescending(p => p.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountHistoryAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _context.PomodoroSessions.CountAsync(p => p.UserId == userId && p.EndTime != null, cancellationToken);

    public async Task<(int sessions, int totalMinutes)> GetStatsAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var query = _context.PomodoroSessions
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.EndTime != null && p.StartTime >= fromUtc && p.StartTime <= toUtc);

        var sessions = await query.CountAsync(cancellationToken);
        var totalMinutes = await query.SumAsync(p => (int?)p.DurationMinutes, cancellationToken) ?? 0;
        return (sessions, totalMinutes);
    }

    public async Task AddAsync(PomodoroSession session, CancellationToken cancellationToken = default) =>
        await _context.PomodoroSessions.AddAsync(session, cancellationToken);

    public Task UpdateAsync(PomodoroSession session, CancellationToken cancellationToken = default)
    {
        _context.PomodoroSessions.Update(session);
        return Task.CompletedTask;
    }
}
