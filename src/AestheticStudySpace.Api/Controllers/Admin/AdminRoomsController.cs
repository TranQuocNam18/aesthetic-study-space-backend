using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Rooms;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/rooms")]
[Authorize(Roles = "Admin")]
public class AdminRoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public AdminRoomsController(IRoomService roomService) => _roomService = roomService;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoomDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RoomDetailDto>>> Create([FromBody] CreateRoomRequestDto request, CancellationToken cancellationToken)
    {
        var room = await _roomService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<RoomDetailDto>.Ok(room, "Room created."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoomDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RoomDetailDto>>> Update(Guid id, [FromBody] UpdateRoomRequestDto request, CancellationToken cancellationToken)
    {
        var room = await _roomService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<RoomDetailDto>.Ok(room, "Room updated."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _roomService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Room deleted."));
    }
}
