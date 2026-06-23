using AestheticStudySpace.Application.DTOs.Report;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IReportService
{
    /// <summary>
    /// Creates and persists a new report from the authenticated user,
    /// then sends an email notification to the support inbox.
    /// </summary>
    Task<ReportResponseDto> CreateReportAsync(Guid userId, CreateReportRequestDto request, CancellationToken cancellationToken = default);
}
