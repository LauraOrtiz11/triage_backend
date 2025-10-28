using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;

namespace triage_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationService _service;

        public ConsultationController(IConsultationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Inicia una consulta médica desde el triage seleccionado.
        /// Cambia el estado del triage a "Finalizado", registra la fecha y crea la consulta asociada.
        /// </summary>
        /// <param name="model">Datos del triage y médico que inicia la consulta.</param>
        /// <returns>Devuelve el ID de la consulta creada.</returns>
        /// <response code="200">La consulta fue iniciada correctamente.</response>
        /// <response code="400">Datos inválidos o error al iniciar la consulta.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("start")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult StartConsultation([FromBody] StartConsultationDto model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Datos inválidos recibidos." });

            try
            {
                var consultationId = _service.StartConsultation(model);

                if (consultationId > 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Consulta iniciada correctamente.",
                        consultationId //  Devolvemos el ID de la consulta creada
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se pudo iniciar la consulta."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }
    }
}
