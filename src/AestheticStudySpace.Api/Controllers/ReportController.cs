using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Report;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

/// <summary>
/// Endpoints for authenticated users to submit bug reports or issue reports.
/// Reports are saved in the database and a notification email is sent to the support inbox.
/// </summary>
[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
        => _reportService = reportService;

    /// <summary>
    /// Submit a new report/issue. The report will be saved and an email notification
    /// will be sent to the support team. Any authenticated user can submit a report.
    /// </summary>
    /// <remarks>
    /// - **Title**: A short summary of the issue (max 256 characters).
    /// - **Content**: A detailed description of the issue (max 4000 characters).
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReportResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReportResponseDto>>> CreateReport(
        [FromBody] CreateReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.CreateReportAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<ReportResponseDto>.Ok(result, "Your report has been submitted successfully. Our team will review it shortly."));
    }
}
