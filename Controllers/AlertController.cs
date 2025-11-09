using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;

namespace triage_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertController : ControllerBase
    {
        private readonly IAlertService _service;

        public AlertController(IAlertService service)
        {
            _service = service;
        }

        /// <summary>
        /// Registra una alerta de empeoramiento realizada por el paciente.
        /// </summary>
        [HttpPost("notify-deterioration")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult NotifyDeterioration([FromBody] CreateAlertDto dto)
        {
            _service.RegisterAlert(dto);
            return Ok("Notificación de empeoramiento registrada correctamente.");
        }

        /// <summary>
        /// Obtiene todas las notificaciones de empeoramiento registradas.
        /// Si no hay alertas, devuelve un mensaje indicando que no hay alertas.
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(List<AlertDetailDto>), 200)]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult GetAllAlerts()
        {
            var alerts = _service.GetAllAlerts();

            if (alerts == null || alerts.Count == 0)
                return Ok("No hay alertas en este momento.");

            return Ok(alerts);
        }

        /// <summary>
        /// Actualiza el estado de una alerta (1: Pendiente, 2: Atendido/finalizado).
        /// </summary>
        [HttpPut("{idAlert}/status/{idStatus}")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult UpdateAlertStatus(int idAlert, int idStatus)
        {
            _service.UpdateAlertStatus(idAlert, idStatus);
            return Ok("Estado de la alerta actualizado correctamente.");
        }
    }
}
