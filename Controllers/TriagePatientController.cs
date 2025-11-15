using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triage_backend.Services;
using triage_backend.Utilities;

namespace triage_backend.Controllers
{
    /// <summary>
    /// Controlador responsable de gestionar las operaciones de pacientes del triage.
    /// Proporciona acceso a los datos del triage filtrados por color de prioridad.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Authorize(Roles = RoleConstants.NURSE)]
    public class TriagePatientController : ControllerBase
    {
        private readonly ITriagePatientService _triageService;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TriagePatientController"/>.
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación para la conexión a la base de datos.</param>
        public TriagePatientController(IConfiguration configuration)
        {
            var context = new ContextDB(configuration);
            _triageService = new TriageService(context);
        }

        /// <summary>
        /// Obtiene una lista de pacientes clasificados, opcionalmente filtrados por color de prioridad.
        /// </summary>
        /// <param name="color">
        /// (Opcional) Color de prioridad para filtrar los pacientes del triage.
        /// Valores válidos: 'rojo', 'naranja', 'amarillo', 'verde', 'azul'.
        /// </param>
        /// <returns>
        /// Una lista de pacientes con información del triage y prioridad asignada.
        /// </returns>
        /// <response code="200">Devuelve la lista de pacientes del triage.</response>
        /// <response code="400">Si ocurre un error durante el proceso.</response>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public IActionResult GetPatients([FromQuery] string? color)
        {
            try
            {
                var patients = _triageService.GetTriagePatients(color);
                return Ok(new { success = true, data = patients });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
