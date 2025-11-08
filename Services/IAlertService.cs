using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface IAlertService
    {
        void RegisterAlert(CreateAlertDto dto);
        List<AlertDetailDto> GetAllAlerts();
        void UpdateAlertStatus(int idAlert, int idStatus);
    }
}
