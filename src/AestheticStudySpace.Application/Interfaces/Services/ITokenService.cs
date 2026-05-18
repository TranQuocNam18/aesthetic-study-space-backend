using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiration();
}
