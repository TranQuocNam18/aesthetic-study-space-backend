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

    // ── Store catalog ──────────────────────────────────────────────────────────

    /// <summary>
    /// Store catalog — all categories. Optionally filter by category.
    /// Accepts category as string name (e.g. "Theme", "Background") or integer.
    /// </summary>
    [HttpGet("catalog")]
    [AllowAnonymous]
    public Task<ActionResult<ApiResponse<PagedResult<StoreItemDto>>>> GetCatalog(
        [FromQuery] StoreCategory? category,
        [FromQuery] StoreThemeSource? themeSource,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetItemsInternal(category, themeSource, StoreCatalogScope.All, page, pageSize, cancellationToken);

    /// <summary>Browse all items (alias for /catalog, backward-compatible).</summary>
    [HttpGet]
    [AllowAnonymous]
    public Task<ActionResult<ApiResponse<PagedResult<StoreItemDto>>>> GetCatalogRoot(
        [FromQuery] StoreCategory? category,
        [FromQuery] StoreThemeSource? themeSource,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetItemsInternal(category, themeSource, StoreCatalogScope.All, page, pageSize, cancellationToken);

    /// <summary>Browse themes only (room background themes).</summary>
    [HttpGet("themes")]
    [AllowAnonymous]
    public Task<ActionResult<ApiResponse<PagedResult<StoreItemDto>>>> GetThemes(
        [FromQuery] StoreThemeSource? themeSource,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetItemsInternal(StoreCategory.Theme, themeSource, StoreCatalogScope.ThemesOnly, page, pageSize, cancellationToken);

    /// <summary>Browse assets only (backgrounds, stickers, effects, ambient sounds).</summary>
    [HttpGet("assets")]
    [AllowAnonymous]
    public Task<ActionResult<ApiResponse<PagedResult<StoreItemDto>>>> GetAssets(
        [FromQuery] StoreCategory? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetItemsInternal(category, null, StoreCatalogScope.AssetsOnly, page, pageSize, cancellationToken);

    /// <summary>Get a single store item by ID.</summary>
    [HttpGet("items/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<StoreItemDto>>> GetItem(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeService.GetItemAsync(id, TryGetViewerUserId(), cancellationToken);
        return Ok(ApiResponse<StoreItemDto>.Ok(item));
    }

    // ── Current user — inventory & history ────────────────────────────────────

    /// <summary>
    /// Current user's owned items (inventory).
    /// Requires authentication.
    /// </summary>
    [HttpGet("me/inventory")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserInventoryItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserInventoryItemDto>>>> GetMyInventory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var inventory = await _storeService.GetInventoryAsync(User.GetUserId(), page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserInventoryItemDto>>.Ok(inventory));
    }

    /// <summary>
    /// Current user's purchase &amp; transaction history.
    /// Filter by <c>kind</c>: <c>StoreItem</c> | <c>CoinPack</c> | <c>Subscription</c>.
    /// Leave blank to return all transaction types.
    /// </summary>
    [HttpGet("me/purchases")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PurchaseHistoryItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PurchaseHistoryItemDto>>>> GetMyPurchases(
        [FromQuery] PurchaseHistoryKind? kind,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var history = await _storeService.GetPurchaseHistoryAsync(User.GetUserId(), kind, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<PurchaseHistoryItemDto>>.Ok(history));
    }

    // ── Backward-compatible aliases ────────────────────────────────────────────

    /// <summary>Alias for /me/inventory (backward compatible).</summary>
    [HttpGet("inventory")]
    [Authorize]
    public Task<ActionResult<ApiResponse<PagedResult<UserInventoryItemDto>>>> GetInventoryLegacy(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetMyInventory(page, pageSize, cancellationToken);

    /// <summary>Alias for /me/purchases (backward compatible).</summary>
    [HttpGet("purchases")]
    [Authorize]
    public Task<ActionResult<ApiResponse<PagedResult<PurchaseHistoryItemDto>>>> GetPurchasesLegacy(
        [FromQuery] PurchaseHistoryKind? kind,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetMyPurchases(kind, page, pageSize, cancellationToken);

    // ── Purchasing ─────────────────────────────────────────────────────────────

    /// <summary>Purchase a store item using coins.</summary>
    [HttpPost("purchase")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<StorePurchaseResultDto>>> PurchaseWithCoins(
        [FromBody] BuyWithCoinsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _storeService.BuyWithCoinsAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<StorePurchaseResultDto>.Ok(result));
    }

    // ── Internal helper ────────────────────────────────────────────────────────

    private async Task<ActionResult<ApiResponse<PagedResult<StoreItemDto>>>> GetItemsInternal(
        StoreCategory? category,
        StoreThemeSource? themeSource,
        StoreCatalogScope scope,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var catalog = await _storeService.GetCatalogAsync(category, themeSource, scope, TryGetViewerUserId(), page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<StoreItemDto>>.Ok(catalog));
    }
}
