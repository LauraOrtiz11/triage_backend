using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Interfaces;

namespace triage_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriorityUpdateController : ControllerBase
    {
        private readonly IPriorityUpdateService _service;

        public PriorityUpdateController(IPriorityUpdateService service)
        {
            _service = service;
        }



        /// <summary>
        /// Devuelve el estado actual del triage del paciente (buscando automáticamente su triage activo).
        /// </summary>
        /// <param name="idPatient">Identificador del paciente</param>
        /// <returns>Información completa del triage activo para el paciente.</returns>
        [HttpGet("status/patient/{idPatient}")]
        public IActionResult GetPatientStatusByPatient(int idPatient)
        {
            var data = _service.GetPatientStatusByPatient(idPatient);

            if (data == null)
                return Ok(new { Success = false, Message = "El paciente no tiene un triage activo o registrado." });

            return Ok(new
            {
                Success = true,
                Message = "Estado del paciente obtenido correctamente.",
                Data = data
            });
        }

    }
}
