using triage_backend.Dtos;
using triage_backend.Services;
using Microsoft.AspNetCore.Mvc;


namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosisController : ControllerBase
    {
        private readonly IDiagnosisService _diagnosisService;

        public DiagnosisController(IDiagnosisService diagnosisService)
        {
            _diagnosisService = diagnosisService;
        }

        [HttpPost("get-by-id")]
        public async Task<IActionResult> GetDiagnosisById([FromBody] DiagnosisRequestDto request)
        {
            if (request == null || request.DiagnosisId <= 0)
                return BadRequest(new { message = "ID de diagnóstico no válido." });

            var diagnosis = await _diagnosisService.GetDiagnosisByIdAsync(request.DiagnosisId);

            if (diagnosis == null)
                return NotFound(new { message = "Diagnóstico no encontrado." });

            return Ok(diagnosis);
        }
    }
}
