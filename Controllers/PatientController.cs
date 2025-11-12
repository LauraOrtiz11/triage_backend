using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;
using triage_backend.Utilities;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleConstants.NURSE)]

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
        [HttpPost("create")]
        public IActionResult CreatePatient([FromBody] PatientDto patientDto)
        {
            if (patientDto == null)
                return BadRequest(new { Success = false, Message = "Los datos del paciente no pueden ser nulos." });

            var result = _patientService.CreatePatient(patientDto);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint para obtener información básica del paciente por cédula.
        /// </summary>
        [HttpPost("get-by-document")]
        public IActionResult GetPatientByDocument([FromBody] PatientDocumentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DocumentIdPt))
                return BadRequest(new { Success = false, Message = "Debe ingresar el número de identificación." });

            var patient = _patientService.GetPatientByDocument(request.DocumentIdPt);

            if (patient == null)
                return NotFound(new { Success = false, Message = "No se encontró ningún paciente con esa cédula." });

            return Ok(new
            {
                Success = true,
                Data = patient
            });
        }


    }
}
