using Microsoft.AspNetCore.Mvc;
using triage_backend.Dtos;
using triage_backend.Services;
using System;
using System.Collections.Generic;

namespace triage_backend.Controllers
{
    /// <summary>
    /// Controlador que gestiona la información completa del triage y el historial médico del paciente.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TriageFullInfoController : ControllerBase
    {
        private readonly ITriageFullInfoService _service;

        public TriageFullInfoController(ITriageFullInfoService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene los detalles completos de un triage específico.
        /// </summary>
        /// <param name="triageId">ID del triage seleccionado.</param>
        /// <returns>Información del triage y paciente asociado.</returns>
        /// <response code="200">Datos del triage encontrados correctamente.</response>
        /// <response code="404">No se encontró el triage solicitado.</response>
        /// <response code="400">El parámetro proporcionado no es válido.</response>
        [HttpGet("details/{triageId:int}")]
        public ActionResult<TriageFullInfoDto.TriageDetailsDto> GetTriageDetails(int triageId)
        {
            try
            {
                var result = _service.GetTriageDetailsById(triageId);

                if (result == null)
                    return NotFound(new { success = false, message = "No se encontró información para el triage solicitado." });

                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno del servidor.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todo el historial clínico del paciente (consultas, diagnósticos y tratamientos).
        /// </summary>
        /// <param name="patientId">ID del paciente obtenido del triage.</param>
        /// <returns>Lista con el historial médico del paciente.</returns>
        /// <response code="200">Historial obtenido correctamente.</response>
        /// <response code="404">No se encontró historial para el paciente.</response>
        /// <response code="400">El parámetro proporcionado no es válido.</response>
        [HttpGet("history/{patientId:int}")]
        public ActionResult<IEnumerable<TriageFullInfoDto.PatientHistoryDto>> GetPatientHistory(int patientId)
        {
            try
            {
                var result = _service.GetPatientHistory(patientId);

                if (result == null || result.Count == 0)
                    return NotFound(new { success = false, message = "No se encontró historial clínico para el paciente." });

                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno del servidor.", detail = ex.Message });
            }
        }
    }
}
