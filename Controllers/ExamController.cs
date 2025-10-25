using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Utilities;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Authorize(Roles = RoleConstants.DOCTOR)]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _service;

        public ExamController(IExamService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene todos los exámenes registrados en el sistema.
        /// </summary>
        [HttpGet("get-all")]
        public IActionResult GetAllExams()
        {
            var exams = _service.GetAllExams();

            if (exams == null || !exams.Any())
                return NotFound(new { Success = false, Message = "No hay exámenes registrados en el sistema." });

            return Ok(new { Success = true, Data = exams });
        }

        /// <summary>
        /// Obtiene la información de un examen por su ID (enviado en el body).
        /// </summary>
        [HttpPost("get-by-id")]
        public IActionResult GetExamById([FromBody] ExamIdRequest request)
        {
            if (request == null || request.Id <= 0)
                return BadRequest(new { Success = false, Message = "Debe proporcionar un ID de examen válido." });

            var exam = _service.GetExamById(request.Id);

            if (exam == null)
                return NotFound(new { Success = false, Message = "El examen solicitado no existe." });

            return Ok(new { Success = true, Data = exam });
        }
    }
    
    public class ExamIdRequest
    {
        public int Id { get; set; }
    }
}
