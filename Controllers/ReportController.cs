using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triage_backend.Services;
using triage_backend.Utilities;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Genera un reporte PDF con estadísticas de tiempos promedio de atención en triage.
        /// Solo accesible para administradores.
        /// </summary>
        [HttpGet("triageReport")]
        public IActionResult GenerateReport([FromQuery] string userName, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var reportBytes = _reportService.GenerateTriageReport(userName, startDate, endDate);
            var fileName = _reportService.GetReportFileName(userName);
            return File(reportBytes, "application/pdf", fileName);
        }
    }
}
