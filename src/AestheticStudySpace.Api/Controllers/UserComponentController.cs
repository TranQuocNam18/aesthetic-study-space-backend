using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

/// <summary>
/// Endpoints for authenticated users to submit and manage their own standalone component submissions
/// (Sticker, Background, Effect, AmbientSound). These are individual assets that the user has designed
/// themselves and wants to offer in the store after admin review.
///
/// NOTE: To submit a full Theme combo (which may bundle these components), use POST /api/me/themes.
/// </summary>
[ApiController]
[Route("api/me/components")]
[Authorize]
public class UserComponentController : ControllerBase
{
    private readonly IUserComponentService _componentService;

    public UserComponentController(IUserComponentService componentService)
        => _componentService = componentService;

    /// <summary>
    /// Submit a new self-designed component (Sticker / Background / Effect / AmbientSound) for admin review.
    /// The component will be visible in the store only after an admin approves it.
    /// AssetUrl must be a Cloudinary URL already uploaded by the client (use POST /api/media/upload or /api/media/upload-audio).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserComponentSubmissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserComponentSubmissionDto>>> SubmitComponent(
        [FromBody] SubmitComponentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _componentService.SubmitComponentAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<UserComponentSubmissionDto>.Ok(result, "Component submitted and is pending admin review."));
    }

    /// <summary>
    /// List all standalone components submitted by the current user (any status: PendingReview, Approved, Rejected).
    /// Optional filter by category (Sticker=2, Background=1, Effect=3, AmbientSound=4).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserComponentSubmissionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserComponentSubmissionDto>>>> GetMySubmissions(
        [FromQuery] StoreCategory? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _componentService.GetMySubmissionsAsync(User.GetUserId(), category, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserComponentSubmissionDto>>.Ok(result));
    }

    /// <summary>
    /// Get details of a specific standalone component submission by ID.
    /// Only accessible by the user who submitted it.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserComponentSubmissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserComponentSubmissionDto>>> GetMySubmission(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _componentService.GetMySubmissionByIdAsync(User.GetUserId(), id, cancellationToken);
        return Ok(ApiResponse<UserComponentSubmissionDto>.Ok(result));
    }

    /// <summary>
    /// Withdraw a submitted component. Only allowed if the component is still pending review or was rejected.
    /// An approved (live) component cannot be withdrawn this way.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> WithdrawSubmission(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _componentService.WithdrawSubmissionAsync(User.GetUserId(), id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Component submission withdrawn."));
    }

    /// <summary>
    /// Fully update an existing component submission (PUT). Resets status to PendingReview.
    /// Only allowed when status is PendingReview or Rejected.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserComponentSubmissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserComponentSubmissionDto>>> UpdateComponent(
        Guid id,
        [FromBody] SubmitComponentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _componentService.UpdateAsync(User.GetUserId(), id, request, cancellationToken);
        return Ok(ApiResponse<UserComponentSubmissionDto>.Ok(result, "Component updated successfully."));
    }

    /// <summary>
    /// Partially update an existing component submission (PATCH). Resets status to PendingReview.
    /// Only allowed when status is PendingReview or Rejected.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserComponentSubmissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserComponentSubmissionDto>>> PatchComponent(
        Guid id,
        [FromBody] PatchComponentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _componentService.PatchAsync(User.GetUserId(), id, request, cancellationToken);
        return Ok(ApiResponse<UserComponentSubmissionDto>.Ok(result, "Component updated successfully."));
    }
}
