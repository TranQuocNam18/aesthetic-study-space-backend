using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Assets;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/assets")]
[AllowAnonymous]
[DisableRateLimiting]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService) => _assetService = assetService;

    /// <summary>List assets with optional type and category filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AssetDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AssetDto>>>> GetAll(
        [FromQuery] string? type,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var assets = await _assetService.GetAllAsync(type, category, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AssetDto>>.Ok(assets));
    }
}
