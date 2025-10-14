using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface IMedicListPService
    {
        List<MedicListPDto> GetMedicListP();
    }
}
