using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Workspace;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/workspace")]
[Authorize]
public class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly IWelcomeBackService _welcomeBackService;

    public WorkspaceController(IWorkspaceService workspaceService, IWelcomeBackService welcomeBackService)
    {
        _workspaceService = workspaceService;
        _welcomeBackService = welcomeBackService;
    }

    /// <summary>Get all saved workspace configurations for the current user.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkspaceConfigDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkspaceConfigDto>>>> GetMyWorkspace(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var configs = await _workspaceService.GetMyWorkspaceAsync(userId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WorkspaceConfigDto>>.Ok(configs));
    }

    /// <summary>Save or update workspace configuration for a room.</summary>
    [HttpPost("save")]
    [ProducesResponseType(typeof(ApiResponse<WorkspaceConfigDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WorkspaceConfigDto>>> Save([FromBody] SaveWorkspaceRequestDto request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var config = await _workspaceService.SaveAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<WorkspaceConfigDto>.Ok(config, "Workspace saved."));
    }

    /// <summary>Get personalized AI welcome back message based on yesterday's performance.</summary>
    [HttpGet("welcome-back")]
    [ProducesResponseType(typeof(ApiResponse<WelcomeBackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WelcomeBackDto>>> GetWelcomeBackMessage(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _welcomeBackService.GetWelcomeBackMessageAsync(userId, cancellationToken);
        return Ok(ApiResponse<WelcomeBackDto>.Ok(result));
    }
}
