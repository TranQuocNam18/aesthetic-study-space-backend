using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
        => await _context.Reports.AddAsync(report, cancellationToken);

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Reports
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
}
