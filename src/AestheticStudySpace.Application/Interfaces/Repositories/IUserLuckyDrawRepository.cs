using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IUserLuckyDrawRepository
{
    Task<IReadOnlyList<UserLuckyDraw>> GetDrawsForDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default);
    Task<int> CountDrawsForDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default);
    Task AddAsync(UserLuckyDraw luckyDraw, CancellationToken cancellationToken = default);
}
