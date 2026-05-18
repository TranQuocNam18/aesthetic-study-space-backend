using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Assets;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/assets")]
[Authorize(Roles = "Admin")]
public class AdminAssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AdminAssetsController(IAssetService assetService) => _assetService = assetService;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AssetDto>>> Create([FromBody] CreateAssetRequestDto request, CancellationToken cancellationToken)
    {
        var asset = await _assetService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<AssetDto>.Ok(asset, "Asset created."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AssetDto>>> Update(Guid id, [FromBody] UpdateAssetRequestDto request, CancellationToken cancellationToken)
    {
        var asset = await _assetService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AssetDto>.Ok(asset, "Asset updated."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _assetService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Asset deleted."));
    }
}
