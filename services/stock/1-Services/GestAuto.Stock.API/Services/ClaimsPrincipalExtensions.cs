using System.Security.Claims;
using GestAuto.Stock.Domain.Exceptions;

namespace GestAuto.Stock.API.Services;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
        {
            throw new UnauthorizedException("User identifier (sub) is missing or invalid.");
        }

        return userId;
    }
}
