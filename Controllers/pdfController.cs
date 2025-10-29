using Microsoft.AspNetCore.Mvc;
using triage_backend.Services;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class pdfController : ControllerBase
    {
        private readonly IpdfService _pdfService;

        public pdfController(IpdfService reportService)
        {
            _pdfService = reportService;
        }

        [HttpGet("triage-report")]
        public IActionResult GenerateTriageReport()
        {
            var pdfBytes = _pdfService.GenerateTriageReport();
            return File(pdfBytes, "application/pdf", "Reporte_Triage.pdf");
        }
    }
}