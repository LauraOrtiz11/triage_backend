using System;
using System.Collections.Generic;
using triage_backend.Dtos;
using triage_backend.Repositories;


namespace triage_backend.Services
{
    /// <summary>
    /// Servicio encargado de la lógica de negocio relacionada con la información completa del triage y el historial del paciente.
    /// </summary>
    public class TriageFullInfoService : ITriageFullInfoService
    {
        private readonly TriageFullInfoRepository _repository;

        public TriageFullInfoService(TriageFullInfoRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Obtiene los detalles completos del triage por su ID.
        /// </summary>
        /// <param name="triageId">ID del triage.</param>
        /// <returns>DTO con la información del triage, o null si no se encuentra.</returns>
        public TriageFullInfoDto.TriageDetailsDto? GetTriageDetailsById(int triageId)
        {
            if (triageId <= 0)
                throw new ArgumentException("El ID del triage debe ser mayor que cero.", nameof(triageId));

            var result = _repository.GetTriageDetailsById(triageId);

            if (result == null || result.TriageId == 0)
                return null;

            return result;
        }

        /// <summary>
        /// Obtiene el historial clínico completo de un paciente.
        /// </summary>
        /// <param name="patientId">ID del paciente.</param>
        /// <returns>Lista con las consultas, diagnósticos y tratamientos del paciente.</returns>
        public List<TriageFullInfoDto.PatientHistoryDto> GetPatientHistory(int patientId)
        {
            if (patientId <= 0)
                throw new ArgumentException("El ID del paciente debe ser mayor que cero.", nameof(patientId));

            var result = _repository.GetPatientHistory(patientId);

            return result ?? new List<TriageFullInfoDto.PatientHistoryDto>();
        }
    }
}
