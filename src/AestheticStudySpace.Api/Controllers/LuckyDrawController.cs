using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.LuckyDraw;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/lucky-draw")]
[Authorize]
public class LuckyDrawController : ControllerBase
{
    private readonly ILuckyDrawService _luckyDrawService;

    public LuckyDrawController(ILuckyDrawService luckyDrawService)
    {
        _luckyDrawService = luckyDrawService;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<LuckyDrawStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LuckyDrawStatusDto>>> GetStatus(CancellationToken cancellationToken = default)
    {
        var status = await _luckyDrawService.GetStatusAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<LuckyDrawStatusDto>.Ok(status));
    }

    [HttpPost("spin")]
    [ProducesResponseType(typeof(ApiResponse<LuckyDrawResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LuckyDrawResultDto>>> Spin(CancellationToken cancellationToken = default)
    {
        var result = await _luckyDrawService.SpinAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<LuckyDrawResultDto>.Ok(result, "Daily lucky draw completed!"));
    }
}
