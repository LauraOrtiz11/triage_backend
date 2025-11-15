using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;
using triage_backend.Utilities;

namespace triage_backend.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    [Authorize(Roles = RoleConstants.ADMIN)]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _service;

        public DashboardController(DashboardService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene los tiempos promedio de atención y de espera de los pacientes, agrupados por hora, dentro del rango de fechas indicado.
        /// </summary>
        /// 
        [HttpPost("average-times")]
        public IActionResult GetAverageTimes([FromBody] DashboardFilterDto filter)
        {
            var data = _service.GetAverageTimes(filter);
            return Ok(data);
        }

        /// <summary>
        /// Devuelve el número total de pacientes atendidos por semana dentro del rango de fechas indicado.
        /// </summary>
        [HttpPost("attentions")]
        public IActionResult GetAttentionsPerWeek([FromBody] DashboardFilterDto filter)
        {
            var data = _service.GetAttentionsPerWeek(filter);
            return Ok(data);
        }

        /// <summary>
        /// Muestra la distribución porcentual de los pacientes según su nivel de prioridad asignado en el triage.
        /// </summary>
        [HttpPost("priority-distribution")]
        public IActionResult GetPriorityDistribution([FromBody] DashboardFilterDto filter)
        {
            var data = _service.GetPriorityDistribution(filter);
            return Ok(data);
        }

        /// <summary>
        /// Obtiene los diagnósticos más frecuentes registrados en el sistema durante el rango de fechas especificado.
        /// </summary>
        [HttpPost("diagnosis-frequency")]
        public IActionResult GetDiagnosisFrequency([FromBody] DashboardFilterDto filter)
        {
            var data = _service.GetDiagnosisFrequency(filter);
            return Ok(data);
        }

        /// <summary>
        /// Lista todos los usuarios con rol Enfermera/o (ID_ROL = 2).
        /// </summary>
        [HttpGet("nurses")]
        [ProducesResponseType(typeof(List<UserBasicDto>), 200)]
        public IActionResult GetNurses()
        {
            var data = _service.GetNurses();
            return Ok(data);
        }

        /// <summary>
        /// Lista todos los usuarios con rol Médico/a (ID_ROL = 3).
        /// </summary>
        [HttpGet("doctors")]
        [ProducesResponseType(typeof(List<UserBasicDto>), 200)]
        public IActionResult GetDoctors()
        {
            var data = _service.GetDoctors();
            return Ok(data);
        }

    }
}