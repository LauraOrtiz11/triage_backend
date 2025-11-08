using triage_backend.Dtos;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class AlertService : IAlertService
    {
        private readonly AlertRepository _repository;

        public AlertService(AlertRepository repository)
        {
            _repository = repository;
        }

        public void RegisterAlert(CreateAlertDto dto)
            => _repository.RegisterAlert(dto);

        public List<AlertDetailDto> GetAllAlerts()
            => _repository.GetAllAlerts();

        public void UpdateAlertStatus(int idAlert, int idStatus)
            => _repository.UpdateAlertStatus(idAlert, idStatus);
    }
}
