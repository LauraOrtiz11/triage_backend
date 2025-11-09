using triage_backend.Dtos;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class DashboardService
    {
        private readonly DashboardRepository _repository;

        public DashboardService(DashboardRepository repository)
        {
            _repository = repository;
        }

        public List<AvgTimesDto> GetAverageTimes(DashboardFilterDto filter)
        {
            return _repository.GetAverageTimes(filter);
        }

        public List<AttentionsDto> GetAttentionsPerWeek(DashboardFilterDto filter)
        {
            return _repository.GetAttentionsPerWeek(filter);
        }

        public List<PriorityDistributionDto> GetPriorityDistribution(DashboardFilterDto filter)
        {
            return _repository.GetPriorityDistribution(filter);
        }

        public List<DiagnosisFrequencyDto> GetDiagnosisFrequency(DashboardFilterDto filter)
        {
            return _repository.GetDiagnosisFrequency(filter);
        }
    }
}
