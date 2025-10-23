using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class TreatmentService : ITreatmentService
    {
        private readonly TreatmentRepository _repository;

        public TreatmentService(TreatmentRepository repository)
        {
            _repository = repository;
        }

        public int RegisterTreatment(TreatmentRequestDto request)
        {
            return _repository.RegisterTreatment(request);
        }
    }
}
