using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using triage_backend.Dtos;
using triage_backend.Interfaces;

namespace triage_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TriageController : ControllerBase
    {
        private readonly IHuggingFaceService _huggingFaceService;

        public TriageController(IHuggingFaceService huggingFaceService)
        {
            _huggingFaceService = huggingFaceService;
        }

        [HttpPost("predict")]
        public async Task<ActionResult<TriageResponseDto>> PredictTriage([FromBody] TriageRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Symptoms))
            {
                return BadRequest("Symptoms and vital signs are required.");
            }

            var prediction = await _huggingFaceService.GetTriagePredictionAsync(request);
            return Ok(prediction);
        }
    }
}
