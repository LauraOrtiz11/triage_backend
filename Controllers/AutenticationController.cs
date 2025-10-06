using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using triage_backend.Dtos;
using triage_backend.Services;
using triage_backend.Utilities;
using triage_backend.Repositories;

namespace triage_backend.Controllers
{
    public class AutenticationController : Controller
    {

        private readonly IAutenticationService _autenticationService;
        private readonly ITokenService _tokenService;
        private readonly IRevokedTokenRepository _revokedRepo;

        public AutenticationController(IAutenticationService autenticationService, ITokenService tokenService, IRevokedTokenRepository revokedRepo)
        {
            _autenticationService = autenticationService;
            _tokenService = tokenService;
            _revokedRepo = revokedRepo ?? throw new ArgumentNullException(nameof(revokedRepo));
        }


        /// <summary>
        /// Endpoint para login de usuario.
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                return BadRequest(new { message = "Email y password requeridos" });

            // 1) Traer usuario por email (devuelve UserDto con IdUs, EmailUs, PasswordHashUs, RoleIdUs, etc.)
            var user = _autenticationService.GetByEmail(loginDto.Email);
            if (user == null)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

            // 2) Validar que exista el hash guardado (evita warning y NullReference)
            if (string.IsNullOrEmpty(user.PasswordHashUs))
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

            // 3) Verificar la contraseña con la utilidad de encriptado
            var isValid = EncryptUtility.VerifyPassword(loginDto.Password, user.PasswordHashUs);
            if (!isValid)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

            // 4) Preparar roles: si tienes lista Roles úsala; sino usa RoleIdUs como fallback
            IEnumerable<string>? roles = null;
            if (user.Roles != null && user.Roles.Any())
            {
                roles = user.Roles;
            }
            else
            {
                roles = new List<string> { user.RoleNameUs! };
            }

            // 5) Generar token 
            var token = _tokenService.CreateToken(user.IdUs?.ToString() ?? string.Empty, user.EmailUs, roles);

            return Ok(new
            {
                Success = true,
                Token = token,
                ExpiresAt = _tokenService.GetExpiry(),
                User = new
                {
                    Id = user.IdUs,
                    Email = user.EmailUs,
                    user.FirstNameUs,
                    user.LastNameUs,
                    user.RoleIdUs,
                    RoleName = user.RoleNameUs,
                    Roles = user.Roles
                }
            });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var user = HttpContext.User;

            var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            return Ok(new
            {
                NameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Name = user.FindFirst(ClaimTypes.Name)?.Value,
                JwtId = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value,
                Roles = roles,
                AllClaims = claims
            });
        }
        // ------------------ Logout: revoca el token actual ------------------
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
                return BadRequest(new { success = false, message = "Token does not contain jti" });

            DateTime? expiresAt = null;
            var exp = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
            if (long.TryParse(exp, out var expUnix))
                expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

            await _revokedRepo.AddAsync(jti, expiresAt);

            return Ok(new { success = true, message = "Token revoked" });
        }
    }
}

