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

    /// <summary>Tổng doanh thu phân loại theo loại thanh toán (Subscription, CoinPack, Asset).</summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<AdminRevenueSummaryDto>>> Revenue(CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetRevenueSummaryAsync(cancellationToken);
        return Ok(ApiResponse<AdminRevenueSummaryDto>.Ok(result));
    }

    /// <summary>Doanh thu theo từng ngày trong khoảng `days` ngày gần nhất (mặc định 30 ngày, tối đa 365).</summary>
    [HttpGet("revenue-trend")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminRevenueTrendDto>>>> RevenueTrend([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetRevenueTrendAsync(days, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminRevenueTrendDto>>.Ok(result));
    }
}

