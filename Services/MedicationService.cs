using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly MedicationRepository _repository;

        public MedicationService(MedicationRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<MedicationDto> GetAllMedications()
        {
            return _repository.GetAllMedications();
        }

        public MedicationDto? GetMedicationById(int id)
        {
            return _repository.GetMedicationById(id);
        }
    }
}
