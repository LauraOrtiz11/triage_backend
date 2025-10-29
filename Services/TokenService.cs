using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace triage_backend.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        
        // expiraciones (puedes mover a appsettings)
        private readonly TimeSpan _accessTokenLifetime = TimeSpan.FromMinutes(15); // ajustar si quieres
        private readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(30);

        public TokenService(IConfiguration config)
        {
            _config = config;
            
        }

        public string CreateAccessToken(string userId, string email, IEnumerable<string>? roles = null)
        {
            // Obtener valores desde la configuración
            var key = _config["Jwt:Key"] ?? throw new Exception("Jwt:Key not set");
            var issuer = _config["Jwt:Issuer"] ?? "triage_backend";
            var audience = _config["Jwt:Audience"] ?? "triage_backend_users";

            // Claims básicos
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Añadir roles 
            if (roles != null)
            {
                foreach (var r in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, r));
                }
            }

            // Generar la clave de firmado
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = DateTime.UtcNow.AddMinutes(15);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public DateTime GetAccessExpiry() => DateTime.UtcNow.Add(_accessTokenLifetime);

        public string CreateRefreshToken()
        {
            // Genera un refresh token fuerte (base64-url)
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        public DateTime GetRefreshExpiry() => DateTime.UtcNow.Add(_refreshTokenLifetime);


    }
}
