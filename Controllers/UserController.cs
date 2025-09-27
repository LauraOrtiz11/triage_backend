using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            return Ok(result);
        }


        /// <summary>
        /// Cambia el estado de un usuario (habilitar o deshabilitar).
        /// Si el usuario tiene procesos activos no podrá ser deshabilitado.
        /// </summary>
        [HttpPut("ChangeStatus/{userId}")]
        public IActionResult ChangeStatus(int userId, [FromQuery] int newState)
        {
            var result = _userService.ChangeUserStatus(userId, newState);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todos los usuarios o los filtra por cédula/nombre.
        /// </summary>
        /// <param name="searchTerm">Texto de búsqueda (cédula o nombre)</param>
        /// <returns>Lista de usuarios</returns>
        [HttpGet]
        public IActionResult GetUsers([FromQuery] string? searchTerm = null)
        {
            var users = _userService.GetUsers(searchTerm);
            return Ok(users);
        }
    }
}
