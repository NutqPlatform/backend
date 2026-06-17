using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Nutq.Core.Auth
{
    public static class JwtTokenGenerator
    {
        private const string SecretKey = "ThisIsAVeryLongSecretKeyForJwtSigningThatIsAtLeast32Characters"; // In production, use configuration
        private const string Issuer = "NutqApp";
        private const string Audience = "NutqUsers";
        private const int ExpirationMinutes = 1440; // 24 hours

        public static string GenerateToken(int userId, string email, string role, string? name = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("name", name ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static DateTime GetExpirationTime()
        {
            return DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        }
    }
}
