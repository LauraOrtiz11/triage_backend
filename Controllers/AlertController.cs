using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;
using triage_backend.Utilities;

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
        [Authorize(Roles = RoleConstants.PATIENT)]
        [HttpPost("notify-deterioration")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult NotifyDeterioration([FromBody] CreateAlertDto dto)
        {
            _service.RegisterAlert(dto);
            return Ok("Notificación de empeoramiento registrada correctamente.");
        }

        /// <summary>
        /// Obtiene todas las notificaciones de empeoramiento registradas.
        /// Solo el enfermero debe verlas.
        /// </summary>
        [Authorize(Roles = RoleConstants.NURSE)]
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
        /// Actualiza el estado de una alerta.
        /// Solo el enfermero puede actualizarla.
        /// </summary>
        [Authorize(Roles = RoleConstants.NURSE)]
        [HttpPut("{idAlert}/status/{idStatus}")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult UpdateAlertStatus(int idAlert, int idStatus)
        {
            _service.UpdateAlertStatus(idAlert, idStatus);
            return Ok("Estado de la alerta actualizado correctamente.");
        }
    }
}
