using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;
using triage_backend.Utilities;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleConstants.ADMIN)]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Endpoint para crear un nuevo usuario.
        /// </summary>
        /// <param name="userDto">Datos del usuario a registrar.</param>
        [HttpPost("create")]
        public IActionResult CreateUser([FromBody] UserDto userDto)
        {
            var result = _userService.CreateUser(userDto);
            dynamic r = result;

            if (!r.Success)
                return BadRequest(new { message = r.Message });

            return Ok(new { message = r.Message, userId = r.UserId });
        }

        /// <summary>
        /// Obtiene todos los usuarios o los filtra por cédula/nombre.
        /// </summary>
        [HttpGet]
        public IActionResult GetUsers([FromQuery] string? searchTerm = null)
        {
            var users = _userService.GetUsers(searchTerm);

            if (users == null || !users.Any())
                return NotFound(new { message = "No se encontraron usuarios." });

            return Ok(new { message = "Usuarios obtenidos correctamente.", data = users });
        }

        /// <summary>
        /// Obtiene la información de un usuario por ID (para edición).
        /// </summary>
        [HttpGet("GetUserById/{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _userService.GetUserById(id);

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado." });

            return Ok(new { message = "Usuario obtenido correctamente.", data = user });
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// </summary>
        [HttpPut("UpdateUser/{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserDto user)
        {
            if (user == null || id != user.UserId)
                return BadRequest(new { message = "Los datos del usuario no son válidos." });

            var result = _userService.UpdateUser(user);

            if (!result.Success)
                return Conflict(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Cambia el estado de un usuario (habilitar o deshabilitar).
        /// Si el usuario tiene procesos activos no podrá ser deshabilitado.
        /// </summary>
        [HttpPut("ChangeStatus/{userId}")]
        public IActionResult ChangeStatus(int userId, [FromQuery] int newState)
        {
            var result = _userService.ChangeUserStatus(userId, newState);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}