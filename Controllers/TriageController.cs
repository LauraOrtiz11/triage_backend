using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;
using triage_backend.Utilities;

namespace triage_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Authorize(Roles = RoleConstants.NURSE)]
    public class TriageController : ControllerBase
    {
        private readonly TriageDataService _triageService;

        public TriageController(TriageDataService triageService)
        {
            _triageService = triageService;
        }

        /// <summary>
        /// Registra un nuevo proceso de Triage a un paciente, el enfermero digita los signos vitales y sintomas.
        /// El modelo de inteligencia artificial retorna una clasificación estimada.
        /// </summary>

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