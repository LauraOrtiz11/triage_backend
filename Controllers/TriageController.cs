using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;

namespace triage_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TriageController : ControllerBase
    {
        private readonly TriageDataService _triageService;

        public TriageController(TriageDataService triageService)
        {
            _triageService = triageService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<TriageResponseDto>> RegisterTriage([FromBody] TriageRequestDto request)
        {
            if (request == null)
                return BadRequest("La solicitud está vacía.");

            try
            {
                var result = await _triageService.ProcessTriageAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error procesando triage: {ex.Message}");
            }
        }
    }
}
