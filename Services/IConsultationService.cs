using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface IConsultationService
    {
        bool StartConsultation(StartConsultationDto model);
    }
}
