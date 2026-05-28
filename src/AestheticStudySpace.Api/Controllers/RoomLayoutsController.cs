using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.DTOs.RoomLayouts;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/room-layouts")]
[Authorize]
public class RoomLayoutsController : ControllerBase
{
    private readonly IRoomLayoutService _roomLayoutService;

    public RoomLayoutsController(IRoomLayoutService roomLayoutService) => _roomLayoutService = roomLayoutService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoomLayoutDto>>>> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _roomLayoutService.GetMyLayoutsAsync(userId, page, pageSize, cancellationToken);
        return ApiResponse<IReadOnlyList<RoomLayoutDto>>.Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoomLayoutDto>>> Create([FromBody] SaveRoomLayoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _roomLayoutService.SaveAsync(userId, null, request, cancellationToken);
        return ApiResponse<RoomLayoutDto>.Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoomLayoutDto>>> Update([FromRoute] Guid id, [FromBody] SaveRoomLayoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _roomLayoutService.SaveAsync(userId, id, request, cancellationToken);
        return ApiResponse<RoomLayoutDto>.Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _roomLayoutService.DeleteAsync(userId, id, cancellationToken);
        return ApiResponse<object>.Ok(new { deleted = true });
    }

    [HttpPost("{id:guid}/duplicate")]
    public async Task<ActionResult<ApiResponse<RoomLayoutDto>>> Duplicate([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _roomLayoutService.DuplicateAsync(userId, id, cancellationToken);
        return ApiResponse<RoomLayoutDto>.Ok(result);
    }
}

