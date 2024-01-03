using System.Security.Claims;

namespace Eventhat.Helpers;

public static class ClaimsExtensions
{
    public static Guid? Id(this ClaimsPrincipal c)
    {
        var value = c.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value == null)
            return default;
        return Guid.Parse(value);
    }
}