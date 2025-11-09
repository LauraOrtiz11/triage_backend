using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;

namespace triage_backend.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
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
    }
}
