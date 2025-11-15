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
    [Authorize(Roles = RoleConstants.DOCTOR)]
    public class MedicListPController : ControllerBase
    {
        private readonly IMedicListPService _service;

        public MedicListPController(IMedicListPService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Obtiene la lista de pacientes activos con su información de triage y médico tratante.
        /// Se puede filtrar por nombre completo o número de cédula.
        /// </summary>
        /// <param name="filter">Opcional: filtros de nombre o cédula</param>
        [HttpPost("GetAllFiltered")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(500)]
        public IActionResult GetAllFiltered([FromBody] MedicListFilterDto? filter)
        {
            try
            {
                var data = _service.GetMedicListP(filter);

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
