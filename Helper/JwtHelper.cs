using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendPBPI.Helper
{
    public class JwtHelper
    {
        public static string? GetUserRole(ClaimsPrincipal user)
        {
            // Asumsi role disimpan dalam claim "role" atau "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            var roleClaim = user.FindFirst(ClaimTypes.Role) ?? user.FindFirst("role");
            return roleClaim?.Value;
        }

        public static int GetUserId(ClaimsPrincipal user)
        {
            // Asumsi UserID disimpan dalam claim "sub", "nameid" atau "userId"
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                ?? user.FindFirst("sub")
                ?? user.FindFirst("userId");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User ID not found in token");
        }

        public static bool IsAdmin(ClaimsPrincipal user)
        {
            var role = GetUserRole(user);
            return role?.Equals("1", StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}