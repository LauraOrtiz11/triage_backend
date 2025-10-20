using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Interfaces;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriorityUpdateController : ControllerBase
    {
        private readonly IPriorityUpdateService _service;

        public PriorityUpdateController(IPriorityUpdateService service)
        {
            _service = service;
        }

      

        /// <summary>
        /// Devuelve el estado actual del triage del paciente.
        /// </summary>
        /// <remarks>
        /// Devuelve la información necesaria para mostrar el estado del triage del paciente, 
        /// incluyendo su turno, prioridad, personal asignado y signos vitales.
        /// </remarks>
        /// <param name="triageId">Identificador del triage</param>
        /// <returns>Información completa del triage para el paciente.</returns>
        [HttpGet("status/{triageId}")]
        public IActionResult GetPatientStatus(int triageId)
        {
            var data = _service.GetPatientStatus(triageId);

            if (data == null)
                return NotFound(new { Success = false, Message = "No se encontró información para este triage." });

            return Ok(new
            {
                Success = true,
                Message = "Estado del paciente obtenido correctamente.",
                Data = data
            });
        }
    }
}
