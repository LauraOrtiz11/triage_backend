using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Utilities;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleConstants.DOCTOR)]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService _service;

        public MedicationController(IMedicationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene todos los medicamentos disponibles en el sistema.
        /// </summary>
        /// <returns>Lista de medicamentos registrados.</returns>
        [HttpGet("get-all")]
        public IActionResult GetAllMedications()
        {
            var medications = _service.GetAllMedications();

            if (medications == null || !medications.Any())
                return NotFound(new
                {
                    Success = false,
                    Message = "No hay medicamentos registrados en el sistema."
                });

            return Ok(new
            {
                Success = true,
                Data = medications
            });
        }

        /// <summary>
        /// Obtiene la información de un medicamento específico mediante su ID.
        /// </summary>
        /// <param name="request">Objeto que contiene el identificador del medicamento.</param>
        /// <returns>Datos del medicamento encontrado.</returns>
        [HttpPost("get-by-id")]
        public IActionResult GetMedicationById([FromBody] MedicationIdRequestDto request)
        {
            if (request == null || request.Id <= 0)
                return BadRequest(new
                {
                    Success = false,
                    Message = "Debe proporcionar un ID de medicamento válido."
                });

            var medication = _service.GetMedicationById(request.Id);

            if (medication == null)
                return NotFound(new
                {
                    Success = false,
                    Message = "El medicamento solicitado no existe."
                });

            return Ok(new
            {
                Success = true,
                Data = medication
            });
        }
    }
}