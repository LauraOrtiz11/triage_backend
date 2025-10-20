using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    /// <summary>
    /// Servicio encargado de gestionar la lógica de actualización de prioridad,
    /// generación de turno y consulta del estado actual del paciente.
    /// </summary>
    public class PriorityUpdateService : IPriorityUpdateService
    {
        private readonly PriorityUpdateRepository _repository;

        /// <summary>
        /// Constructor con inyección de dependencias del repositorio.
        /// </summary>
        public PriorityUpdateService(PriorityUpdateRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

      

        /// <summary>
        /// Devuelve el estado actual del paciente, incluyendo su última prioridad y turno.
        /// </summary>
        public PatientStatusDto? GetPatientStatus(int triageId)
        {
            return _repository.GetPatientStatus(triageId);
        }
    }
}
