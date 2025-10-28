using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Interfaces;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreatmentController : ControllerBase
    {
        private readonly ITreatmentService _service;

        public TreatmentController(ITreatmentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Registra un nuevo tratamiento con medicamentos y exámenes asociados.
        /// </summary>
        [HttpPost("register")]
        public IActionResult RegisterTreatment([FromBody] TreatmentRequestDto request)
        {
            if (request == null || request.IdHistory <= 0 || string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { Success = false, Message = "Debe ingresar el historial y la descripción del tratamiento." });

            var id = _service.RegisterTreatment(request);
            if (id <= 0)
                return StatusCode(500, new { Success = false, Message = "Error al registrar el tratamiento en la base de datos." });

            return Ok(new
            {
                Success = true,
                Message = "Tratamiento registrado exitosamente.",
                IdTreatment = id
            });
        }
    }
}
