using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        /// <summary>
        /// Endpoint para crear un nuevo paciente.
        /// </summary>
        /// <param name="patientDto">Datos del paciente a registrar.</param>
        [HttpPost("create")]
        public IActionResult CreatePatient([FromBody] PatientDto patientDto)
        {
            if (patientDto == null)
                return BadRequest(new { Success = false, Message = "Los datos del paciente no pueden ser nulos." });

            var result = _patientService.CreatePatient(patientDto);

            return Ok(result);
        }
    }
}
