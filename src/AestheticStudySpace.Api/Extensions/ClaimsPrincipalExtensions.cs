using System.Security.Claims;

namespace AestheticStudySpace.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(ClaimTypes.Name)
            ?? user.FindFirstValue("sub");

        if (Guid.TryParse(id, out var userId))
            return userId;

        throw new UnauthorizedAccessException("User identifier is missing from token.");
    }
}
