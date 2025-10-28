using triage_backend.Dtos;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class ConsultationService : IConsultationService
    {
        private readonly ConsultationRepository _repository;

        public ConsultationService(ConsultationRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Inicia una nueva consulta médica y devuelve el ID de la consulta creada.
        /// </summary>
        public int StartConsultation(StartConsultationDto dto)
        {
            return _repository.StartConsultation(dto);
        }
    }
}
