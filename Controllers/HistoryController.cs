using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;
using triage_backend.Interfaces;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        /// <summary>
        /// Usando el documento de identidad del paciente, obtiene el ID de la historia médica que le corresponde.
        /// </summary>

        [HttpPost("get-by-document")]
        public async Task<IActionResult> GetHistoryByDocument([FromBody] PatientDocumentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.DocumentIdPt))
                return BadRequest(new { message = "El Documento es requerido." });

            var history = await _historyService.GetHistoryByDocumentAsync(request.DocumentIdPt);

            if (history == null)
                return NotFound(new { message = "No se encontró una historia para el documento ingresado." });

            return Ok(history);
        }

        /// <summary>
        /// Añade un diagnóstico a un historial médico.
        /// </summary>

        [HttpPost("add-diagnosis")]
        public async Task<IActionResult> AddDiagnosisToHistory([FromBody] HistoryDiagnosisRequest request)
        {
            if (request == null || request.HistoryId <= 0 || request.DiagnosisId <= 0)
                return BadRequest(new { message = "Tanto el ID Diagnóstico y el ID Historia son requeridos." });

            bool result = await _historyService.AddDiagnosisToHistoryAsync(request);

            if (!result)
                return Conflict(new { message = "Este diagnóstico ya fue registrado en la historia clínica" });

            return Ok(new { message = "Diagnostico registrado exitosamente." });
        }
    }
}