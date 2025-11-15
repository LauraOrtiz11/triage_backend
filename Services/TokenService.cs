using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using triage_backend.Dtos;
using triage_backend.Interfaces;

namespace triage_backend.Utilities
{
    public class TokenService : ITokenService
    {
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public TokenService(IConfiguration config)
        {
            _jwtKey = config["Jwt:Key"]!;
            _jwtIssuer = config["Jwt:Issuer"]!;
            _jwtAudience = config["Jwt:Audience"]!;
        }

        public string GenerateToken(AutenticationDto user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Usamos SIEMPRE el rol real según la BD
            string roleName = user.RealRoleName;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, (user.IdUs ?? 0).ToString()),
                new Claim(ClaimTypes.Name, user.EmailUs ?? string.Empty),

                // 🔥 ESTE ES EL CLAIM DECISIVO
                new Claim(ClaimTypes.Role, roleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
