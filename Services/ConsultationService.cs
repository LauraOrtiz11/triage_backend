using triage_backend.Dtos;
using triage_backend.Repositories;
using triage_backend.Services;

namespace triage_backend.Services
{
    public class ConsultationService : IConsultationService
    {
        private readonly ConsultationRepository _repository;

        public ConsultationService(ConsultationRepository repository)
        {
            _repository = repository;
        }

        public bool StartConsultation(StartConsultationDto model)
        {
            return _repository.StartConsultation(model);
        }
    }
}
