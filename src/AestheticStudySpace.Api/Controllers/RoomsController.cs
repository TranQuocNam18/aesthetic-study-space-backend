using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Rooms;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/rooms")]
[AllowAnonymous]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService) => _roomService = roomService;

    /// <summary>List all available study rooms.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoomListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoomListItemDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var rooms = await _roomService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RoomListItemDto>>.Ok(rooms));
    }

    /// <summary>Get room details with default asset layers.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoomDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RoomDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<RoomDetailDto>.Ok(room));
    }
}
