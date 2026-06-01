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
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MissionDto>>>> GetActive(CancellationToken cancellationToken = default)
    {
        var missions = await _missionService.GetActiveAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<MissionDto>>.Ok(missions);
    }

    [HttpPost("{id:guid}/claim")]
    public async Task<ActionResult<ApiResponse<UserMissionDto>>> Claim([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _missionService.ClaimAsync(userId, id, cancellationToken);
        return ApiResponse<UserMissionDto>.Ok(result);
    }
}

