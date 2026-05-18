using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Pomodoro;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/pomodoro")]
[Authorize]
public class PomodoroController : ControllerBase
{
    private readonly IPomodoroService _pomodoroService;

    public PomodoroController(IPomodoroService pomodoroService) => _pomodoroService = pomodoroService;

    [HttpPost("start")]
    [ProducesResponseType(typeof(ApiResponse<PomodoroSessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PomodoroSessionDto>>> Start([FromBody] StartPomodoroRequestDto request, CancellationToken cancellationToken)
    {
        var session = await _pomodoroService.StartAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<PomodoroSessionDto>.Ok(session, "Pomodoro started."));
    }

    [HttpPost("end")]
    [ProducesResponseType(typeof(ApiResponse<PomodoroSessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PomodoroSessionDto>>> End([FromBody] EndPomodoroRequestDto request, CancellationToken cancellationToken)
    {
        var session = await _pomodoroService.EndAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<PomodoroSessionDto>.Ok(session, "Pomodoro ended."));
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PomodoroSessionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PomodoroSessionDto>>>> History(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var history = await _pomodoroService.GetHistoryAsync(User.GetUserId(), page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<PomodoroSessionDto>>.Ok(history));
    }
}
