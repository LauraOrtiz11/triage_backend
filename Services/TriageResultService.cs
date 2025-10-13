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
            // Validación simple
            if (result.TriageId <= 0 || result.PriorityId <= 0 || result.NurseId <= 0)
                throw new ArgumentException("All fields are required");

            return _repository.SaveTriageResult(result);
        }
    }
}
