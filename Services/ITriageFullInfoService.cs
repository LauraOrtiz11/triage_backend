using System.Collections.Generic;
using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface ITriageFullInfoService
    {
        /// <summary>
        /// Obtiene la información completa del triage seleccionado.
        /// </summary>
        /// <param name="triageId">ID del triage.</param>
        /// <returns>Información detallada del triage, o null si no existe.</returns>
        TriageFullInfoDto.TriageDetailsDto? GetTriageDetailsById(int triageId);

        /// <summary>
        /// Obtiene el historial de consultas, diagnósticos y tratamientos de un paciente.
        /// </summary>
        /// <param name="patientId">ID del paciente.</param>
        /// <returns>Lista con el historial clínico del paciente.</returns>
        List<TriageFullInfoDto.PatientHistoryDto> GetPatientHistory(int patientId);
    }
}
