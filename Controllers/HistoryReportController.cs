using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using triage_backend.Utilities;
using TriageBackend.Services;

namespace TriageBackend.Controllers
{
    [ApiController]
    [Route("api/patients/{patientId:int}/[controller]")]
    [Authorize]
    [Authorize(Roles = RoleConstants.PATIENT)]
    public class HistoryReportController : ControllerBase
    {
        private readonly IHistoryReportService _historyReportService;
        private readonly ILogger<HistoryReportController> _logger;

        public HistoryReportController(IHistoryReportService historyReportService, ILogger<HistoryReportController> logger)
        {
            _historyReportService = historyReportService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el ID del usuario desde los claims del token.
        /// </summary>
        private string? GetUserIdFromClaims()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(idClaim)) return idClaim;

            var sub = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrWhiteSpace(sub)) return sub;

            var nameClaim = User.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrWhiteSpace(nameClaim) && int.TryParse(nameClaim, out _)) return nameClaim;

            return null;
        }

        /// <summary>
        /// Valida si el rol es de paciente.
        /// </summary>
        private bool IsPatientRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return false;
            var r = role.Trim().ToLowerInvariant();
            return r == "paciente" || r == "patient";
        }

        
        /// <summary>
        /// Obtiene el historial clínico del paciente.
        /// </summary>
        /// <param name="patientId">ID del paciente al que pertenece el historial.</param>
        /// <param name="from">Fecha inicial opcional.</param>
        /// <param name="to">Fecha final opcional.</param>
        /// <param name="page">Página de resultados.</param>
        /// <param name="limit">Cantidad de registros por página.</param>
        /// <returns>Lista paginada del historial clínico.</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int patientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
                                     [FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var userIdClaim = GetUserIdFromClaims();

            _logger.LogInformation("Get history called. patientId={PatientId}, tokenUserId={TokenUserId}, role={Role}", patientId, userIdClaim, role);

            if (IsPatientRole(role))
            {
                if (!int.TryParse(userIdClaim, out var uid))
                {
                    _logger.LogWarning("Access denied: token missing numeric user id.");
                    return Forbid();
                }

                if (uid != patientId)
                {
                    var resolved = await _historyReportService.GetHistorialIdByUserIdAsync(uid);

                    if (!resolved.HasValue || resolved.Value != patientId)
                    {
                        _logger.LogWarning("Access denied: token user id {TokenUserId} not allowed for historial {PatientId}", uid, patientId);
                        return Forbid();
                    }
                }
            }

            var (items, total) = await _historyReportService.GetPatientHistoryAsync(patientId, from, to, Math.Max(1, page), Math.Clamp(limit, 1, 200));
            return Ok(new { data = items, meta = new { total, page, limit } });
        }

        /// <summary>
        /// Obtiene el detalle de una consulta específica del paciente.
        /// </summary>
        /// <param name="patientId">ID del paciente.</param>
        /// <param name="consultaId">ID de la consulta.</param>
        /// <returns>Detalles de la consulta.</returns>
        [HttpGet("{consultaId:int}")]
        public async Task<IActionResult> GetById(int patientId, int consultaId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var userIdClaim = GetUserIdFromClaims();

            _logger.LogInformation("GetById called. patientId={PatientId}, tokenUserId={TokenUserId}, role={Role}, consultaId={ConsultaId}", patientId, userIdClaim, role, consultaId);

            if (IsPatientRole(role))
            {
                if (!int.TryParse(userIdClaim, out var uid))
                {
                    _logger.LogWarning("Access denied: token missing numeric user id.");
                    return Forbid();
                }

                if (uid != patientId)
                {
                    var resolved = await _historyReportService.GetHistorialIdByUserIdAsync(uid);

                    if (!resolved.HasValue || resolved.Value != patientId)
                    {
                        _logger.LogWarning("Access denied: token user id {TokenUserId} not allowed for historial {PatientId}", uid, patientId);
                        return Forbid();
                    }
                }
            }

            var dto = await _historyReportService.GetConsultationDetailAsync(patientId, consultaId);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Descarga un PDF con el historial clínico del paciente.
        /// </summary>
        /// <param name="patientId">ID del paciente.</param>
        /// <param name="from">Fecha inicial opcional.</param>
        /// <param name="to">Fecha final opcional.</param>
        /// <returns>Archivo PDF generado.</returns>
        [HttpGet("pdf/download")]
        public async Task<IActionResult> DownloadPdf(int patientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var userIdClaim = GetUserIdFromClaims();

            _logger.LogInformation("DownloadPdf called. patientId={PatientId}, tokenUserId={TokenUserId}, role={Role}", patientId, userIdClaim, role);

            if (IsPatientRole(role))
            {
                if (!int.TryParse(userIdClaim, out var uid))
                {
                    _logger.LogWarning("Access denied: token missing numeric user id.");
                    return Forbid();
                }

                if (uid != patientId)
                {
                    var resolved = await _historyReportService.GetHistorialIdByUserIdAsync(uid);

                    if (!resolved.HasValue || resolved.Value != patientId)
                    {
                        _logger.LogWarning("Access denied: token user id {TokenUserId} not allowed for historial {PatientId}", uid, patientId);
                        return Forbid();
                    }
                }
            }

            var pdf = await _historyReportService.GeneratePatientHistoryPdfAsync(patientId, from, to);
            var fileName = $"historial_{patientId}_{(from?.ToString("yyyyMMdd") ?? "desde")}_{(to?.ToString("yyyyMMdd") ?? "hasta")}.pdf";
            return File(pdf, "application/pdf", fileName);
        }
    }
}
