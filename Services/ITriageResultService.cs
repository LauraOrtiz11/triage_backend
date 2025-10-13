using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface ITriageResultService
    {
        bool RegisterTriageResult(TriageResultDto result);
        Task<List<TriageResultPatientInfoDto>> GetPatientTriageInfoAsync(int patientId);
        Task<TriagePriorityInfoDto?> GetPriorityInfoByTriageIdAsync(int triageId);
        Task<List<PriorityInfoDto>> GetAllPrioritiesAsync();

    }
}
