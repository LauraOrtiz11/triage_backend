using Microsoft.AspNetCore.Mvc;
using triage_backend.Interfaces;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService _service;

        public MedicationController(IMedicationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene todos los medicamentos disponibles.
        /// </summary>
        [HttpGet("get-all")]
        public IActionResult GetAllMedications()
        {
            var medications = _service.GetAllMedications();

            if (medications == null || !medications.Any())
                return NotFound(new { Success = false, Message = "No hay medicamentos registrados en el sistema." });

            return Ok(new { Success = true, Data = medications });
        }

        /// <summary>
        /// Obtiene un medicamento por su ID.
        /// </summary>
        [HttpPost("get-by-id")]
        public IActionResult GetMedicationById([FromBody] MedicationIdRequest request)
        {
            if (request == null || request.Id <= 0)
                return BadRequest(new { Success = false, Message = "Debe proporcionar un ID de medicamento válido." });

            var medication = _service.GetMedicationById(request.Id);

            if (medication == null)
                return NotFound(new { Success = false, Message = "El medicamento solicitado no existe." });

            return Ok(new { Success = true, Data = medication });
        }
    }

    /// <summary>
    /// DTO para recibir el ID del medicamento en el body
    /// </summary>
    public class MedicationIdRequest
    {
        public int Id { get; set; }
    }
}
