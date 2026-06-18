using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IUserComponentService
{
    /// <summary>User submits a self-designed component (Sticker/Background/Effect/AmbientSound) for admin review.</summary>
    Task<UserComponentSubmissionDto> SubmitComponentAsync(Guid userId, SubmitComponentRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Returns all standalone (non-inline) component submissions by the user (any status). Optional category filter.</summary>
    Task<PagedResult<UserComponentSubmissionDto>> GetMySubmissionsAsync(Guid userId, StoreCategory? category, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Get a single standalone component submission owned by the user.</summary>
    Task<UserComponentSubmissionDto> GetMySubmissionByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Withdraw a standalone component submission (soft-delete).
    /// Only allowed when Status is PendingReview or Rejected.
    /// </summary>
    Task WithdrawSubmissionAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>Fully update a standalone component submission (PUT). Resets status to PendingReview.</summary>
    Task<UserComponentSubmissionDto> UpdateAsync(Guid userId, Guid id, SubmitComponentRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Partially update a standalone component submission (PATCH). Resets status to PendingReview.</summary>
    Task<UserComponentSubmissionDto> PatchAsync(Guid userId, Guid id, PatchComponentRequestDto request, CancellationToken cancellationToken = default);
}
