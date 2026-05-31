using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Rooms;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/my/rooms")]
[Authorize]
public class UserRoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public UserRoomsController(IRoomService roomService) => _roomService = roomService;

    /// <summary>Get all custom rooms created by the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoomListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoomListItemDto>>>> GetMine(CancellationToken cancellationToken)
    {
        var rooms = await _roomService.GetMyRoomsAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RoomListItemDto>>.Ok(rooms));
    }

    /// <summary>
    /// Create a new custom room.
    /// Free users are limited to 3 rooms; Premium users have unlimited rooms.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoomDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RoomDetailDto>>> Create(
        [FromBody] UserCreateRoomRequestDto request,
        CancellationToken cancellationToken)
    {
        var room = await _roomService.CreateUserRoomAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<RoomDetailDto>.Ok(room, "Room created."));
    }

    /// <summary>Update one of the current user's custom rooms.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoomDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RoomDetailDto>>> Update(
        Guid id,
        [FromBody] UserUpdateRoomRequestDto request,
        CancellationToken cancellationToken)
    {
        var room = await _roomService.UpdateUserRoomAsync(User.GetUserId(), id, request, cancellationToken);
        return Ok(ApiResponse<RoomDetailDto>.Ok(room, "Room updated."));
    }

    /// <summary>Delete one of the current user's custom rooms.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _roomService.DeleteUserRoomAsync(User.GetUserId(), id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Room deleted."));
    }
}
