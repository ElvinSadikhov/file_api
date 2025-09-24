using System.Security.Claims;
using AtomCore.JWT;

namespace Core.Extensions;

public static class JwtTokenHelperExtensions
{
    public static int GetownerIdFromClaims(this JwtTokenHelper jwtTokenHelper)
    {
        return int.Parse(jwtTokenHelper.GetClaim(ClaimTypes.NameIdentifier)!);
    }
}