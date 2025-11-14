using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using triage_backend.Dtos;
using triage_backend.Interfaces;  // 👈 IMPORTANTE: aquí está ITokenService e IAutenticationService
using triage_backend.Repositories;
using triage_backend.Utilities;

namespace triage_backend.Controllers
{
    /// <summary>
    /// Controlador encargado de la autenticación de usuarios.
    /// Maneja el inicio de sesión, obtención del usuario autenticado y cierre de sesión.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticationController : ControllerBase
    {
        private readonly IAutenticationService _authService;
        private readonly ITokenService _tokenService;
        private readonly IRevokedTokenRepository _revokedRepo;

        public AutenticationController(
            IAutenticationService authService,
            ITokenService tokenService,
            IRevokedTokenRepository revokedRepo)
        {
            _authService = authService;
            _tokenService = tokenService;
            _revokedRepo = revokedRepo;
        }

        /// <summary>
        /// Inicia sesión y devuelve la información básica del usuario. 
        /// El token JWT se almacena en una cookie HttpOnly.
        /// </summary>
        /// <param name="loginDto">Credenciales del usuario.</param>
        /// <returns>Información reducida del usuario autenticado.</returns>
        /// <response code="200">Inicio de sesión exitoso.</response>
        /// <response code="400">Datos incompletos o inválidos.</response>
        /// <response code="401">Credenciales incorrectas.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                return BadRequest(new { success = false, message = "Email y password son obligatorios." });

            var user = _authService.GetByEmail(loginDto.Email);
            if (user == null || string.IsNullOrEmpty(user.PasswordHashUs))
                return Unauthorized(new { success = false, message = "Usuario o contraseña incorrectos." });

            if (!EncryptUtility.VerifyPassword(loginDto.Password, user.PasswordHashUs))
                return Unauthorized(new { success = false, message = "Usuario o contraseña incorrectos." });

            // 🔐 Generar token seguro a partir del DTO completo
            var token = _tokenService.GenerateToken(user);

            // 🍪 Guardar token en cookie HttpOnly
            Response.Cookies.Append(
                "X-Auth",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });

            // 🔎 Solo devolvemos datos reducidos al frontend
            return Ok(new
            {
                success = true,
                user = new
                {
                    id = user.IdUs,
                    user.FirstNameUs,
                    user.LastNameUs,
                    user.RoleIdUs
                }
            });
        }

        /// <summary>
        /// Cierra la sesión revocando el token JWT actual.
        /// </summary>
        /// <returns>Confirmación de cierre de sesión.</returns>
        /// <response code="200">Sesión cerrada correctamente.</response>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
                return BadRequest(new { success = false, message = "Token inválido (sin JTI)." });

            var expUnix = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
            DateTime? expiresAt = expUnix != null
                ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expUnix)).UtcDateTime
                : null;

            await _revokedRepo.AddAsync(jti, expiresAt);

            // Borrar cookie HttpOnly
            Response.Cookies.Delete("X-Auth");

            return Ok(new { success = true, message = "Sesión cerrada correctamente." });
        }
    }
}
