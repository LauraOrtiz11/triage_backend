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

        public List<MedicListPDto> GetMedicListP()
        {
            try
            {
                return _repository.GetMedicListP();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving patient triage data: " + ex.Message);
            }
        }
    }
}
