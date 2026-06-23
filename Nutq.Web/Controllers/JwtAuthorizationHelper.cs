using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Nutq.Web.Controllers
{
    public static class JwtAuthorizationHelper
    {
        public static (int UserId, string Role)? GetCurrentUser(HttpRequest request)
        {
            var authHeader = request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            var tokenStr = authHeader.Substring("Bearer ".Length).Trim();
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(tokenStr);
                
                // Get User ID
                var idClaim = token.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == ClaimTypes.NameIdentifier);
                if (idClaim == null || !int.TryParse(idClaim.Value, out var userId))
                    return null;

                // Get Role
                var roleClaim = token.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role);
                var role = roleClaim?.Value?.ToLower() ?? "doctor"; 

                return (userId, role);
            }
            catch
            {
                return null;
            }
        }
    }
}
