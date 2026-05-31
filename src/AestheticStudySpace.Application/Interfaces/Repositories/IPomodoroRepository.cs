using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IPomodoroRepository
{
    Task<PomodoroSession?> GetActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PomodoroSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PomodoroSession>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(int sessions, int totalMinutes)> GetStatsAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task AddAsync(PomodoroSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(PomodoroSession session, CancellationToken cancellationToken = default);
    Task DeleteAsync(PomodoroSession session, CancellationToken cancellationToken = default);
}
