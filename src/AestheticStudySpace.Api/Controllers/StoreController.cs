using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/store")]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoreController(IStoreService storeService) => _storeService = storeService;

    [HttpGet("items")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StoreItemDto>>>> GetItems(
        [FromQuery] StoreCategory? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var items = await _storeService.GetItemsAsync(category, page, pageSize, cancellationToken);
        return ApiResponse<IReadOnlyList<StoreItemDto>>.Ok(items);
    }

    [HttpPost("buy/coins")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> BuyWithCoins([FromBody] BuyWithCoinsRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _storeService.BuyWithCoinsAsync(userId, request, cancellationToken);
        return ApiResponse<object>.Ok(result);
    }
}

