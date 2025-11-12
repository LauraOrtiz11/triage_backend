using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace triage_backend.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private const double TOKEN_DURATION_MINUTES = 30; 

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateToken(string userId, string email, IEnumerable<string>? roles = null)
        {
            // Obtener valores desde la configuración
            var key = _config["Jwt:Key"] ?? throw new Exception("Jwt:Key not set in configuration");
            var issuer = _config["Jwt:Issuer"] ?? throw new Exception("Jwt:Issuer not set in configuration");
            var audience = _config["Jwt:Audience"] ?? throw new Exception("Jwt:Audience not set in configuration");

            // Claims básicos
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Añadir roles (si existen)
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // Generar clave de firmado
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            //  Expira exactamente en 30 minutos
            var expires = DateTime.UtcNow.AddMinutes(TOKEN_DURATION_MINUTES);

            // Crear el token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public DateTime GetExpiry() => DateTime.UtcNow.AddMinutes(TOKEN_DURATION_MINUTES);
    }
}
