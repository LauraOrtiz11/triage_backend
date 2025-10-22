using triage_backend.Dtos;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class TriageResultService : ITriageResultService
    {
        private readonly TriageResultRepository _repository;

        public TriageResultService(TriageResultRepository repository)
        {
            _repository = repository;
        }

        public bool RegisterTriageResult(TriageResultDto result)
        {
            // === Validaciones de datos ===
            if (result == null)
                throw new ArgumentException("Los datos enviados son inválidos.");

            if (result.TriageId <= 0)
                throw new ArgumentException("El identificador del triage es obligatorio.");

            if (result.PriorityId <= 0)
                throw new ArgumentException("Debe seleccionar un nivel de prioridad válido.");

            if (result.NurseId <= 0)
                throw new ArgumentException("El identificador del enfermero es obligatorio.");

            return _repository.SaveTriageResult(result);
        }

      
        // Listar la informacion detallada del paciente
        public async Task<List<TriageResultPatientInfoDto>> GetPatientTriageInfoAsync(int triageId)
        {
            if (triageId <= 0)
                throw new ArgumentException("Id del triage invalido");

            return await _repository.GetPatientTriageInfoAsync(triageId);
        }
        // Lista la prioridad y nombre asociado a un triage
        public async Task<TriagePriorityInfoDto?> GetPriorityInfoByTriageIdAsync(int triageId)
        {
            return await _repository.GetPriorityInfoByTriageIdAsync(triageId);
        }

        // Lista las prioridades disponibles con su descripcion
        public async Task<List<PriorityInfoDto>> GetAllPrioritiesAsync()
        {
            return await _repository.GetAllPrioritiesAsync();
        }

    }
}
