using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Admin;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminAnalyticsController(IAdminService adminService) => _adminService = adminService;

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<AdminOverviewDto>>> Overview(CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetOverviewAsync(cancellationToken);
        return Ok(ApiResponse<AdminOverviewDto>.Ok(result));
    }

    [HttpGet("user-growth")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminDateCountDto>>>> UserGrowth([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetUserGrowthAsync(days, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminDateCountDto>>.Ok(result));
    }

    [HttpGet("feature-usage")]
    public async Task<ActionResult<ApiResponse<AdminFeatureUsageDto>>> FeatureUsage(CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetFeatureUsageAsync(cancellationToken);
        return Ok(ApiResponse<AdminFeatureUsageDto>.Ok(result));
    }
}

