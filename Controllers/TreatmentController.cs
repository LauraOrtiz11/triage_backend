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

        [HttpPost("register")]
        public IActionResult RegisterTreatment([FromBody] TreatmentRequestDto request)
        {
            if (request == null || request.IdDiagnosis <= 0 || string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { Success = false, Message = "Debe ingresar el diagnóstico y la descripción del tratamiento." });

            var ok = _service.RegisterTreatment(request);
            if (!ok)
                return StatusCode(500, new { Success = false, Message = "Error al registrar el tratamiento en la base de datos." });

            return Ok(new { Success = true, Message = "Tratamiento registrado exitosamente." });
        }
    }
}
