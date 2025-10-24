using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly HistoryRepository _repository;

        public HistoryService(HistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<HistoryResponseDto?> GetHistoryByDocumentAsync(string documentId)
        {
            return await _repository.GetHistoryByDocumentAsync(documentId);
        }

        public async Task<bool> AddDiagnosisToHistoryAsync(HistoryDiagnosisRequest request)
        {
            
            return await _repository.AddDiagnosisToHistoryAsync(request);
        }
    }
}
