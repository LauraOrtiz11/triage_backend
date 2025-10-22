
using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;

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

        // 🔹 Get History by Patient Document
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

        // 🔹 Add Diagnosis to a History
        [HttpPost("add-diagnosis")]
        public async Task<IActionResult> AddDiagnosisToHistory([FromBody] HistoryDiagnosisRequest request)
        {
            if (request == null || request.HistoryId <= 0 || request.DiagnosisId <= 0)
                return BadRequest(new { message = "Tanto el ID Diagnóstico y el ID Historia son requeridos." });

            bool result = await _historyService.AddDiagnosisToHistoryAsync(request);

            if (!result)
                return Conflict(new { message = "This diagnosis is already registered for the selected history." });

            return Ok(new { message = "Diagnosis successfully registered to history." });
        }
    }
}
