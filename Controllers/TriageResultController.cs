using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;

namespace triage_backend.Controllers
{
    /// <summary>
    /// Controlador encargado de registrar los resultados finales del triaje.
    /// Permite al personal de enfermería confirmar o ajustar la prioridad sugerida por la IA
    /// y almacenar la trazabilidad del registro.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TriageResultController : ControllerBase
    {
        private readonly ITriageResultService _triageResultService;

        /// <summary>
        /// Constructor del controlador con inyección de dependencias.
        /// </summary>
        /// <param name="triageResultService">Servicio para manejar resultados de triaje.</param>
        public TriageResultController(ITriageResultService triageResultService)
        {
            _triageResultService = triageResultService;
        }

        /// <summary>
        /// Registra (inserta) el resultado final del triaje confirmado o ajustado por el enfermero.
        /// </summary>
   
        /// <param name="result">DTO con los datos del resultado de triaje (en body).</param>
        /// <returns>Objeto con success y message.</returns>
        /// <response code="200">Resultado registrado exitosamente.</response>
        /// <response code="400">Datos inválidos o error al registrar.</response>
        [HttpPost("register")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public IActionResult RegisterTriageResult([FromBody] TriageResultDto result)
        {
            try
            {
                bool success = _triageResultService.RegisterTriageResult(result);

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Resultado de triaje registrado exitosamente."
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "No se pudo registrar el resultado de triaje."
                });
            }
            catch (ArgumentException aex)
            {
                // Error controlado desde el servicio (validación lógica)
                return BadRequest(new
                {
                    success = false,
                    message = aex.Message
                });
            }
            catch (Exception ex)
            {
                // Error inesperado (base de datos, conexión, etc.)
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor: " + ex.Message
                });
            }
        }


        /// <summary>
        /// Obtiene la información del triage activo de un paciente segun el ID del triage.
        /// </summary>
        /// <param name="request">Objeto con el ID del paciente.</param>
        /// <returns>Lista con la información general del triage.</returns>
        [HttpPost("getTriagePatient")]
        public async Task<IActionResult> GetPatientTriage([FromBody] TriageResultPatientRequestDto request)
        {
            try
            {
                var result = await _triageResultService.GetPatientTriageInfoAsync(request.triageId);
                if (result == null || !result.Any())
                    return NotFound(new { success = false, message = "No se encontró información de triage para este paciente." });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error al obtener datos: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene el nombre y la descripción de la prioridad asignada a un triage específico.
        /// </summary>
        /// <param name="triageId">ID del triage del cual se desea consultar la información de prioridad.</param>
        /// <returns>
        /// Devuelve el nombre y la descripción de la prioridad correspondiente al triage indicado.
        /// </returns>
        /// <response code="200">Retorna la información de la prioridad.</response>
        /// <response code="404">No se encontró información de prioridad para el ID de triage proporcionado.</response>
        [HttpGet("priorityInfo/{triageId:int}")]
        public async Task<IActionResult> GetPriorityInfo(int triageId)
        {
            var result = await _triageResultService.GetPriorityInfoByTriageIdAsync(triageId);

            if (result == null)
                return NotFound(new
                {
                    success = false,
                    message = "No se encontró información de prioridad para este triage."
                });

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        /// <summary>
        /// Obtiene la lista de todas las prioridades registradas en el sistema.
        /// </summary>
        /// <returns>
        /// Devuelve una lista con el nombre y la descripción de cada prioridad.
        /// </returns>
        /// <response code="200">Retorna la lista de prioridades.</response>
        /// <response code="404">No se encontraron prioridades registradas.</response>
        [HttpGet("allPriorities")]
        public async Task<IActionResult> GetAllPriorities()
        {
            var priorities = await _triageResultService.GetAllPrioritiesAsync();

            if (priorities == null || priorities.Count == 0)
                return NotFound(new
                {
                    success = false,
                    message = "No se encontraron prioridades registradas en el sistema."
                });

            return Ok(new
            {
                success = true,
                data = priorities
            });
        }

    }
}
