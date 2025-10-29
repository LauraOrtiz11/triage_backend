using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using triage_backend.Dtos;
using triage_backend.Services;
using triage_backend.Utilities;
using triage_backend.Repositories;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace triage_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
        public class AutenticationController : Controller
    {

        private readonly IAutenticationService _autenticationService;
        private readonly ITokenService _tokenService;
        private readonly IRevokedTokenRepository _revokedRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AutenticationController(IAutenticationService autenticationService, ITokenService tokenService, IRevokedTokenRepository revokedRepo, IRefreshTokenRepository refreshTokenRepository)
        {
            _autenticationService = autenticationService;
            _tokenService = tokenService;
            _revokedRepo = revokedRepo ?? throw new ArgumentNullException(nameof(revokedRepo));
            _refreshTokenRepository = refreshTokenRepository;
        }


        /// <summary>
        /// Endpoint para login de usuario.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult>Login([FromBody] LoginDto loginDto)
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
            var accessToken = _tokenService.CreateAccessToken(user.IdUs?.ToString() ?? string.Empty, user.EmailUs, roles);

            // 6) Crear refresh token (aleatorio)
            var refreshToken = _tokenService.CreateRefreshToken();
            var refreshExpires = _tokenService.GetRefreshExpiry();

            // 7) Guardar refresh token en DB (hash) asociado al usuario (implementa SaveRefreshToken)
            await _refreshTokenRepository.SaveAsync(user.IdUs.Value, refreshToken, refreshExpires);

            //8) Setear cookie HttpOnly con refresh token
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // true en prod con HTTPS
                SameSite = SameSiteMode.None, // si frontend y backend en dominios distintos
                Expires = refreshExpires
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            // Devuelve solo user info (no tokens visibles). Puedes devolver expires del access si quieres.
            var userInfo = new
            {
                Id = user.IdUs,
                Email = user.EmailUs,
                FirstName = user.FirstNameUs,
                LastName = user.LastNameUs,
                RoleId = user.RoleIdUs,
                RoleName = user.RoleNameUs,
                Roles = user.Roles
            };

            return Ok(new { Success = true, ExpiresAt = _tokenService.GetAccessExpiry(), User = userInfo });
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
        
        // Endpoint para renovar access token usando la cookie HttpOnly (refresh token)
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return Unauthorized(new { message = "No refresh token" });

            // Aquí asumimos que la cookie está asociada a un user: necesitas extraer el userId
            // Si guardaste userId en tabla RefreshTokens, puedes buscar por tokenHash y obtener userId.
            // Implementaremos una simple validación: buscar el userId por tokenHash (modificar repo si quieres)
            // Para simplicidad, hacemos un select adicional: validar y obtener userId a partir del token
            // -> Vamos a añadir un método ValidateAndGetUserAsync en el repo (implementaré abajo)
            var userId = await _refreshTokenRepository.GetUserIdIfValidAsync(refreshToken);
            if (userId == null) return Unauthorized(new { message = "Invalid refresh token" });

            // obtener user minimal info desde AutenticationService
            var user = _autenticationService.GetById(userId.Value); // implementa este método o usa user repo
            if (user == null) return Unauthorized();

            var roles = user.Roles != null && user.Roles.Any() ? user.Roles : new List<string> { user.RoleNameUs! };

            var newAccessToken = _tokenService.CreateAccessToken(user.IdUs?.ToString() ?? string.Empty, user.EmailUs, roles);
            // opcional: rotate refresh token (crear uno nuevo, guardar y sustituir cookie)
            return Ok(new
            {
                Success = true,
                AccessToken = newAccessToken,
                ExpiresAt = _tokenService.GetAccessExpiry()
            });
        }
        // ------------------ Logout: revoca el token actual ------------------
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // 1) Revocar access token por jti
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
            {
                DateTime? expiresAt = null;
                var exp = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
                if (long.TryParse(exp, out var expUnix))
                    expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                await _revokedRepo.AddAsync(jti, expiresAt);
            }

            // 2) Revocar refresh token desde la cookie

            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {       
                // elimina registro de refresh token de BD
                await _refreshTokenRepository.RevokeAsync(refreshToken);
                // borrar cookie
                Response.Cookies.Delete("refreshToken", new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None });
            }
            return Ok(new { success = true, message = "Token revoked / Cookie cleared" }); ;
        }
    }
}

