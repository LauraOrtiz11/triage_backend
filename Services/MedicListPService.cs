using triage_backend.Dtos;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class MedicListPService : IMedicListPService
    {
        private readonly MedicListPRepository _repository;

        public MedicListPService(MedicListPRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<MedicListPDto> GetMedicListP(MedicListFilterDto? filter = null)
        {
            return _repository.GetMedicListP(filter);
        }
    }
}
