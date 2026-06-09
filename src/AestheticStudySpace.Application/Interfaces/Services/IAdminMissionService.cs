using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Missions;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IAdminMissionService
{
    Task<PagedResult<AdminMissionDto>> GetMissionsAsync(bool includeInactive, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminMissionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminMissionDto> CreateAsync(CreateMissionRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminMissionDto> UpdateAsync(Guid id, UpdateMissionRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
