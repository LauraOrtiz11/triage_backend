using Microsoft.AspNetCore.Mvc;
using triage_backend.Services;

namespace triage_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicListPController : ControllerBase
    {
        private readonly IMedicListPService _service;

        // ✅ Inyección de dependencias correcta (el servicio se inyecta automáticamente)
        public MedicListPController(IMedicListPService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Obtiene la lista de pacientes activos con su información de triage y médico tratante.
        /// </summary>
        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(500)]
        public IActionResult GetAll()
        {
            try
            {
                var data = _service.GetMedicListP();

                if (data == null || data.Count == 0)
                    return Ok(new { success = true, message = "No hay pacientes registrados actualmente." });

                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener los datos de triage.",
                    detail = ex.Message
                });
            }
        }
    }
}
