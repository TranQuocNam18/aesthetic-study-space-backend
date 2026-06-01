using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces.Repositories;
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

    private Guid? TryGetViewerUserId() =>
        User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;

    /// <summary>Store catalog (all categories).</summary>
    [HttpGet]
    [AllowAnonymous]
    public Task<ActionResult<ApiResponse<PagedResult<StoreItemDto>>>> GetCatalog(
        [FromQuery] StoreCategory? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetItemsInternal(category, StoreCatalogScope.All, page, pageSize, cancellationToken);

    [HttpGet("items")]
    [AllowAnonymous]
    public Task<ActionResult<ApiResponse<PagedResult<StoreItemDto>>>> GetItems(
        [FromQuery] StoreCategory? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetItemsInternal(category, StoreCatalogScope.All, page, pageSize, cancellationToken);

    [HttpGet("themes")]
    [AllowAnonymous]
    public Task<ActionResult<ApiResponse<PagedResult<StoreItemDto>>>> GetThemes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetItemsInternal(StoreCategory.Theme, StoreCatalogScope.ThemesOnly, page, pageSize, cancellationToken);

    /// <summary>Backgrounds, stickers, effects, and ambient sounds.</summary>
    [HttpGet("assets")]
    [AllowAnonymous]
    public Task<ActionResult<ApiResponse<PagedResult<StoreItemDto>>>> GetAssets(
        [FromQuery] StoreCategory? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetItemsInternal(category, StoreCatalogScope.AssetsOnly, page, pageSize, cancellationToken);

    [HttpGet("items/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<StoreItemDto>>> GetItem([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _storeService.GetItemAsync(id, TryGetViewerUserId(), cancellationToken);
        return Ok(ApiResponse<StoreItemDto>.Ok(item));
    }

    [HttpGet("inventory")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserInventoryItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserInventoryItemDto>>>> GetInventory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var inventory = await _storeService.GetInventoryAsync(User.GetUserId(), page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserInventoryItemDto>>.Ok(inventory));
    }

    [HttpPost("purchase")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<StorePurchaseResultDto>>> Purchase(
        [FromBody] BuyWithCoinsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _storeService.BuyWithCoinsAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<StorePurchaseResultDto>.Ok(result));
    }

    [HttpPost("buy/coins")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<StorePurchaseResultDto>>> BuyWithCoins(
        [FromBody] BuyWithCoinsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _storeService.BuyWithCoinsAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<StorePurchaseResultDto>.Ok(result));
    }

    [HttpGet("purchases")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PurchaseHistoryItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PurchaseHistoryItemDto>>>> GetPurchaseHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var history = await _storeService.GetPurchaseHistoryAsync(User.GetUserId(), page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<PurchaseHistoryItemDto>>.Ok(history));
    }

    private async Task<ActionResult<ApiResponse<PagedResult<StoreItemDto>>>> GetItemsInternal(
        StoreCategory? category,
        StoreCatalogScope scope,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var catalog = await _storeService.GetCatalogAsync(category, scope, TryGetViewerUserId(), page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<StoreItemDto>>.Ok(catalog));
    }
}
