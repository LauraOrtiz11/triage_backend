using System.Threading.Tasks;
using triage_backend.Dtos;

namespace triage_backend.Interfaces
{
    public interface IHuggingFaceService
    {
        Task<TriageResponseDto> GetTriagePredictionAsync(TriageRequestDto request);
    }
}
