using triage_backend.Dtos;

namespace triage_backend.Interfaces
{
    public interface IHistoryService
    {
        Task<HistoryResponseDto?> GetHistoryByDocumentAsync(string documentId);
        Task<bool> AddDiagnosisToHistoryAsync(HistoryDiagnosisRequest request);
    }
}
