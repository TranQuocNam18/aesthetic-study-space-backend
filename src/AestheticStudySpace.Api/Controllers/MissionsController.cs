using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Missions;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/missions")]
[Authorize]
public class MissionsController : ControllerBase
{
    private readonly IMissionService _missionService;

    public MissionsController(IMissionService missionService) => _missionService = missionService;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MissionWithProgressDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MissionWithProgressDto>>>> GetActive(CancellationToken cancellationToken = default)
    {
        var missions = await _missionService.GetForUserAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MissionWithProgressDto>>.Ok(missions));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MissionWithProgressDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MissionWithProgressDto>>> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var mission = await _missionService.GetByIdForUserAsync(User.GetUserId(), id, cancellationToken);
        return Ok(ApiResponse<MissionWithProgressDto>.Ok(mission));
    }

    [HttpPost("{id:guid}/claim")]
    public async Task<ActionResult<ApiResponse<UserMissionDto>>> Claim([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _missionService.ClaimAsync(userId, id, cancellationToken);
        return ApiResponse<UserMissionDto>.Ok(result);
    }
}

