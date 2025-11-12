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
    /// <summary>
    /// Controlador encargado de la autenticación de usuarios.
    /// Permite iniciar sesión, obtener información del usuario autenticado y cerrar sesión (revocar el token).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticationController : Controller
    {
        private readonly IAutenticationService _autenticationService;
        private readonly ITokenService _tokenService;
        private readonly IRevokedTokenRepository _revokedRepo;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de autenticación.
        /// </summary>
        public AutenticationController(IAutenticationService autenticationService, ITokenService tokenService, IRevokedTokenRepository revokedRepo)
        {
            _autenticationService = autenticationService;
            _tokenService = tokenService;
            _revokedRepo = revokedRepo ?? throw new ArgumentNullException(nameof(revokedRepo));
        }

        /// <summary>
        /// Inicia sesión con las credenciales de usuario.
        /// </summary>
        /// <param name="loginDto">Objeto con el correo electrónico y la contraseña del usuario.</param>
        /// <returns>
        /// Devuelve un token JWT junto con la información básica del usuario autenticado.
        /// </returns>
        /// <response code="200">Inicio de sesión exitoso. Devuelve el token y los datos del usuario.</response>
        /// <response code="400">Si faltan los campos obligatorios o los datos son inválidos.</response>
        /// <response code="401">Si las credenciales son incorrectas.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                return BadRequest(new { message = "Email y password requeridos" });

            var user = _autenticationService.GetByEmail(loginDto.Email);
            if (user == null)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

            if (string.IsNullOrEmpty(user.PasswordHashUs))
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

            var isValid = EncryptUtility.VerifyPassword(loginDto.Password, user.PasswordHashUs);
            if (!isValid)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

            IEnumerable<string>? roles = null;
            if (user.Roles != null && user.Roles.Any())
                roles = user.Roles;
            else
                roles = new List<string> { user.RoleNameUs! };

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

        /// <summary>
        /// Obtiene la información del usuario autenticado mediante el token actual.
        /// </summary>
        /// <returns>
        /// Devuelve los datos básicos del usuario, incluyendo identificador, nombre y roles.
        /// </returns>
        /// <response code="200">Devuelve los datos del usuario autenticado.</response>
        /// <response code="401">Si el token no es válido o ha expirado.</response>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 401)]
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

        /// <summary>
        /// Cierra la sesión del usuario actual y revoca el token JWT utilizado.
        /// </summary>
        /// <returns>
        /// Confirma que el token ha sido revocado correctamente.
        /// </returns>
        /// <response code="200">Token revocado exitosamente.</response>
        /// <response code="400">Si el token no contiene información válida.</response>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> Logout()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
                return BadRequest(new { success = false, message = "El token no contiene el identificador jti" });

            DateTime? expiresAt = null;
            var exp = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
            if (long.TryParse(exp, out var expUnix))
                expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

            await _revokedRepo.AddAsync(jti, expiresAt);

            return Ok(new { success = true, message = "Token revocado correctamente" });
        }
    }
}
