using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;


namespace triage_backend.Controllers
{
    /// <summary>
    /// Controlador encargado de registrar los resultados finales del triaje.
    /// Permite al personal (enfermero/a) confirmar o ajustar la prioridad sugerida por la IA
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
        /// <remarks>
        /// Ejemplo de request JSON:
        ///
        ///     POST /api/TriageResult/register
        ///     {
        ///       "TriageId": 12,
        ///       "PriorityId": 3,
        ///       "NurseId": 8,
        ///       "IsFinalPriority": true
        ///     }
        ///
        /// El endpoint:
        /// - Inserta un nuevo registro en TRIAGE_RESULTADO con Es_Prioridad_Final = 1
        /// - Marca cualquier registro previo para el mismo triage como Es_Prioridad_Final = 0
        /// </remarks>
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
            if (result == null)
                return BadRequest(new { success = false, message = "Request body is required." });

            try
            {
                var success = _triageResultService.RegisterTriageResult(result);

                if (success)
                    return Ok(new { success = true, message = "Resultado de triaje registrado exitosamente." });

                return BadRequest(new { success = false, message = "No se pudo registrar el resultado de triaje." });
            }
            catch (ArgumentException aex)
            {
                return BadRequest(new { success = false, message = aex.Message });
            }
            catch (Exception ex)
            {
                // Para trazabilidad en logs puedes registrar ex.Message / ex.StackTrace aquí
                return BadRequest(new { success = false, message = "Error interno: " + ex.Message });
            }
        }
    }
}
