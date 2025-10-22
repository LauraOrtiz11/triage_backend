using Microsoft.AspNetCore.Mvc;
using triage_backend.Services;

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

        [HttpGet("triageReport")]
        public IActionResult GenerateReport([FromQuery] string userName)
        {
            var reportBytes = _reportService.GenerateTriageReport(userName);
            var fileName = _reportService.GetReportFileName(userName);

            return File(reportBytes, "application/pdf", fileName);
        }

    }
}
