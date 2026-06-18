using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken cancellationToken = default);
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
